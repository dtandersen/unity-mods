using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using Caev;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;
using StationeersAi.Mod.Ai;
using StationeersAi.Mod.Patch;
using StationeersAi.Query;
using StationeersAi.Repository;
using Caev.Tools;
using StationeersAi.Window;
using Util.Commands;
using Caev.OpenAi;
using StationeersAi.Ai;
using StationeersAi.Tool;

namespace StationeersAi.Mod;

/// <summary>
/// Sage BepInEx plugin. The Unity/BepInEx adapter: configuration, the
/// <c>ai</c> console command, and the composition root. All logic lives in
/// <c>StationeersAi.Core</c>.
/// </summary>
[BepInPlugin(ModGuid, ModName, ModVersion)]
public class BepInExPlugin : BaseUnityPlugin, IMainThread
{
    public const string ModGuid = "dtandersen.sage";
    public const string ModName = "SAGE";
    public const string ModVersion = "0.1.0";

    // Harmony patches call back through this; everything else flows through
    // constructors. Set in Awake.
    private static BepInExPlugin _instance;

    private SagePresenter _sage;

    /// <summary>
    /// The core <see cref="Core.Logger"/> handed to core objects for their
    /// trace. Named <c>Log</c> (not <c>Logger</c>) to avoid hiding
    /// <c>BaseUnityPlugin.Logger</c>. Assigned in <see cref="Awake"/> and
    /// never null afterwards.
    /// </summary>
    private BepinExLogger _log;

    private SageConfig _settings;

    private Conversation _conversation;
    private SynchronizationContext _unitySynchronizationContext;
    private int _unityThreadId;

    private MentionResponder _mentionResponder;

    private readonly Dictionary<string, Conversation> _mentionConversations = new Dictionary<string, Conversation>();

    // World data does not depend on the player, so it is built once and
    // reused until the prefab catalogue changes size.
    private IItemRepository _catalogueItems;
    private IStructureRepository _catalogueStructures;
    private IRecipeRepository _catalogueRecipes;
    private IStationpediaRepository _catalogueStationpedia;
    private int _cataloguePrefabCount = -1;

    private Conversation Conversation => _conversation ??= new Conversation(_settings.ContextMessages.Value);

    public void Awake()
    {
        _instance = this;
        if (Logger == null)
        {
            throw new InvalidOperationException($"{ModName}: BepInEx log source is unavailable.");
        }

        _log = new BepinExLogger(Logger);
        _unitySynchronizationContext = SynchronizationContext.Current;
        _unityThreadId = Thread.CurrentThread.ManagedThreadId;
        _log.Log($"captured Unity thread {_unityThreadId}; synchronization context available={_unitySynchronizationContext != null}");

        _settings = new SageConfig(Config);

        if (!_settings.ModEnabled.Value)
        {
            return;
        }

        var harmony = new Harmony(ModGuid);
        harmony.PatchAll(typeof(ChatMessagePatch));
        _log.Log($"patched {harmony.GetPatchedMethods().Count()} method(s) for chat mentions");

        // The window lives on its own persistent object: scene loads destroy
        // the plugin's GameObject, which would silently kill Update/OnGUI.
        // DontDestroyOnLoad keeps the holder alive, so Awake creates it once.
        var holder = new GameObject("SageWindow");
        UnityEngine.Object.DontDestroyOnLoad(holder);
        var window = holder.AddComponent<SageWindow>();
        window.Logger = _log;
        window.WindowToggle = () => _settings.WindowToggle.Value;
        _sage = new SagePresenter(
            new WindowModel(200),
            window,
            activity => BuildAssistant(activity),
            PlayerName,
            () => AssistantName,
            _log,
            () => _settings.ShowThinking.Value,
            () => _settings.ShowToolCalls.Value,
            () => _settings.ShowToolResults.Value,
            () => _settings.ContextMessages.Value > 0 ? Conversation : null,
            Run);
        _log.Log($"Sage window ready ({_settings.WindowToggle.Value})");

        CommandLine.AddCommand("ai", new AiConsoleCommand(_sage, () => AssistantName));
        _log.Log("registered 'ai' console command");

        _log.Log($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
    }

    /// <summary>Runs work on Unity's main thread, posting from async continuations.</summary>
    public void Run(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
        {
            RunGuarded(action);
            return;
        }

        var synchronizationContext = _unitySynchronizationContext;
        if (synchronizationContext == null)
        {
            _log.Log("cannot dispatch to Unity main thread: synchronization context was not captured");
            return;
        }

        synchronizationContext.Post(_ => RunGuarded(action), null);
    }

    private void RunGuarded(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
        }
    }

    /// <summary>
    /// Runs work on Unity's main thread and returns its result, blocking the
    /// caller. Tools run off the main thread, but Unity objects demand it.
    /// Throws when the context was never captured or the wait times out.
    /// </summary>
    public T RunSync<T>(Func<T> func, int timeoutMilliseconds = 10000)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
        {
            return func();
        }

        var synchronizationContext = _unitySynchronizationContext;
        if (synchronizationContext == null)
        {
            throw new InvalidOperationException("Unity synchronization context was not captured.");
        }

        T result = default(T);
        Exception error = null;
        using (var done = new ManualResetEventSlim(false))
        {
            synchronizationContext.Post(_ =>
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done.Set();
                }
            }, null);

            if (!done.Wait(timeoutMilliseconds))
            {
                throw new TimeoutException("Timed out waiting for Unity's main thread.");
            }
        }

        if (error != null)
        {
            throw error;
        }

        return result;
    }

    /// <summary>
    /// Called for every chat message displayed on this machine. If it is a
    /// mention and mention handling is enabled, the question is echoed to
    /// the Sage window and answered there through the window presenter.
    /// Runs on the Unity main thread, so the game-data snapshot is safe
    /// to build. Ignored while another turn is running.
    /// </summary>
    /// <summary>Harmony entry: chat messages displayed on this machine.</summary>
    public static void OnChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        _instance?.HandleChatMessage(message);
    }

    private void HandleChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        if (!_settings.ModEnabled.Value || !_settings.RespondToMentions.Value || message == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message.ChatText))
        {
            return;
        }

        // Skip our own answers echoing back through the server relay.
        if (message.DisplayName != null &&
            message.DisplayName.Equals(AssistantName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _log.Log($"chat seen from '{message.DisplayName}': {Truncate(message.ChatText, 80)}");

        if (!Mention.TryParse(_settings.MentionTrigger.Value, message.ChatText, out var prompt))
        {
            return;
        }

        _log.Log($"mention matched, asking: {Truncate(prompt, 80)}");
        var responder = MentionResponderInstance();
        if (responder.IsBusy)
        {
            _log.Log("mention ignored: Sage is already answering");
            return;
        }

        _sage.Show();
        var speaker = string.IsNullOrWhiteSpace(message.DisplayName)
            ? "player"
            : message.DisplayName;
        _sage.Write($"{speaker}: {prompt}");
        _ = responder.HandleAsync(prompt, MentionConversation(message.DisplayName));
    }

    /// <summary>One chat session per player, capped like the console history.</summary>
    private Conversation MentionConversation(string player)
    {
        if (_settings.ContextMessages.Value <= 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(player))
        {
            player = "player";
        }

        if (!_mentionConversations.TryGetValue(player, out var conversation))
        {
            conversation = new Conversation(_settings.ContextMessages.Value);
            _mentionConversations[player] = conversation;
        }

        return conversation;
    }

    internal static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
        {
            return value;
        }

        return value.Substring(0, max) + "...";
    }

    private MentionResponder MentionResponderInstance()
    {
        return _mentionResponder ??= new MentionResponder(
            new FanoutChatSink(_sage, new ChatMessageSink(this, _log, () => AssistantName)),
            activity => BuildAssistant(activity),
            AssistantName,
            _log);
    }

    /// <summary>
    /// Display name for the assistant. Configurable via the
    /// <c>General/Name</c> entry; falls back to "Sage" when blank.
    /// </summary>
    public string AssistantName => ChatNaming.Resolve(_settings.AiName?.Value);

    private string PlayerName()
    {
        try
        {
            var human = Human.LocalHuman;
            if (human != null && !string.IsNullOrWhiteSpace(human.CustomName))
            {
                return human.CustomName;
            }
        }
        catch (Exception)
        {
        }

        return null;
    }

    /// <summary>
    /// World data (items, structures, recipes, Stationpedia) does not depend on
    /// the player, so it is built once and reused until the prefab catalogue
    /// changes (game load, mods adding prefabs).
    /// </summary>
    private void EnsureCatalogues()
    {
        var prefabs = Prefab.AllPrefabs;
        var count = prefabs != null ? prefabs.Count : 0;
        if (_catalogueItems != null && _cataloguePrefabCount == count && count > 0)
        {
            return;
        }

        _catalogueItems = new StationeersItemRepository(prefabs);
        _catalogueStructures = new StationeersStructureRepository(prefabs);
        _catalogueRecipes = new StationeersRecipeRepository();
        _catalogueStationpedia = new StationeersStationpediaRepository(Stationpedia.StationpediaPages);
        _cataloguePrefabCount = count;
        _log.Log($"rebuilt world catalogues ({count} prefabs)");
    }

    private AiAssistant BuildAssistant(IAssistantActivity activity)
    {
        var prefabs = Prefab.AllPrefabs;
        if (prefabs == null || prefabs.Count == 0)
        {
            return null;
        }

        EnsureCatalogues();

        Human human = null;
        try
        {
            human = Human.LocalHuman;
        }
        catch (Exception)
        {
        }

        var playerContext = new StationeersPlayerContext(human);

        var tools = new ToolHandler(new ITool[]
        {
            new SearchItemsTool(new SearchItemsCommand(new ItemIndex(_catalogueItems))),
            new SearchStructuresTool(new SearchStructuresCommand(new StructureIndex(_catalogueStructures))),
            new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(_catalogueRecipes))),
            new SearchStationpediaTool(new SearchStationpediaCommand(new StationpediaIndex(_catalogueStationpedia))),
            new ReadStationpediaTool(new ReadStationpediaCommand(_catalogueStationpedia)),
            new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(playerContext))),
            new GetStatusTool(new GetStatusCommand(playerContext)),
            new GetWorldTool(new GetWorldCommand(new StationeersWorldContext())),
            new WriteIcTool(new WriteIcCommand(new StationeersIcEditor(this, _log))),
            new ReadIcTool(new ReadIcCommand(new StationeersIcEditor(this, _log))),
            new ExportIcTool(new ExportIcCommand(new StationeersIcExport(this, _log))),
            new MipsReferenceTool(new ReadMipsCommand(new StationeersMipsReference(_log))),
            new SearchDevicesTool(new SearchDevicesCommand(new StationeersDeviceCatalog(this, _log))),
            new DescribeDeviceTool(new DescribeDeviceCommand(new StationeersDeviceInspector(this, _log), new StationeersDeviceCatalog(this, _log))),
        });

        var client = new OpenAiChatClient(
            new UnityWebRequestTransport(_settings.TimeoutSeconds.Value),
            new OpenAiOptions(_settings.Endpoint.Value, _settings.ApiKey.Value, _settings.Model.Value, _settings.ReasoningEffort.Value));

        return new AiAssistant(client, tools, SystemPromptText(), _settings.MaxIterations.Value, _log, activity);
    }

    /// <summary>
    /// The system prompt: <c>SYSTEM.md</c> beside the config when it exists
    /// (re-read per question, so edits apply without a restart), else the
    /// configured text.
    /// </summary>
    private string SystemPromptText()
    {
        try
        {
            var path = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "SYSTEM.md");
            if (System.IO.File.Exists(path))
            {
                var text = System.IO.File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Log($"SYSTEM.md unreadable, using configured prompt: {ex.Message}");
        }

        return _settings.SystemPrompt.Value;
    }
}
