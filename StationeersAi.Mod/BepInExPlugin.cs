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
using StationeersAi.Mod.Server;
using StationeersAi.Server;
using StationeersAi.Tool;

namespace StationeersAi.Mod;

/// <summary>
/// Sage BepInEx plugin. The Unity/BepInEx adapter: configuration, the
/// <c>ai</c> console command, and the composition root. All logic lives in
/// <c>StationeersAi.Core</c>.
/// </summary>
[BepInPlugin(ModGuid, ModName, ModVersion)]
public class BepInExPlugin : BaseUnityPlugin
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

    private MentionResponder _mentionResponder;
    private MainThreadExecutor _threads;

    // Process-owned: Stationeers wipes scene objects (even DontDestroyOnLoad
    // ones) on world load, which runs OnDestroy on the plugin. A static
    // server outlives that wipe; nothing here needs a live GameObject.
    private static SageHttpServer _httpServer;

    private static readonly object _httpLock = new object();
    private WindowModel _windowModel;
    private SageImGuiWindow _window;

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
        var synchronizationContext = SynchronizationContext.Current;
        var unityThreadId = Thread.CurrentThread.ManagedThreadId;
        _threads = new MainThreadExecutor(synchronizationContext, unityThreadId, _log);
        _log.Log($"captured Unity thread {unityThreadId}; synchronization context available={synchronizationContext != null}");

        _settings = new SageConfig(Config);

        if (!_settings.ModEnabled.Value)
        {
            return;
        }

        var harmony = new Harmony(ModGuid);
        harmony.PatchAll(typeof(ChatMessagePatch));
        harmony.PatchAll(typeof(SageOverlayPatch));
        _log.Log($"patched {harmony.GetPatchedMethods().Count()} method(s) for chat mentions");

        _windowModel = new WindowModel(200);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, __) => EnsureWindow();
        WorldManager.OnWorldStarted += EnsureWindow;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, __) => EnsureHttpServer();
        WorldManager.OnWorldStarted += EnsureHttpServer;
        EnsureWindow();
        _log.Log($"Sage window ready ({_settings.WindowToggle.Value})");

        CommandLine.AddCommand("ai", new AiConsoleCommand(() => { EnsureWindow(); return _sage; }, () => AssistantName));
        _log.Log("registered 'ai' console command");

        EnsureHttpServer();

        _log.Log($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
    }

    private void OnDestroy()
    {
        // Runs when the scene wipe takes the plugin object on world load.
        // The server is process-owned (static) and keeps serving; the
        // process end closes its socket. Disposing here would kill Pi's
        // access the moment a world loads.
    }

    /// <summary>
    /// Starts the loopback HTTP server for outside tools such as Pi when
    /// <c>Server/Enabled</c> is on, or restarts it when the scene wipe
    /// took it down. Safe to call on every scene load and world start.
    /// Each request reuses the same live repositories the chat reads.
    /// </summary>
    private void EnsureHttpServer()
    {
        lock (_httpLock)
        {
            try
            {
                if (!_settings.ServerEnabled.Value)
                {
                    return;
                }

                if (_httpServer != null && _httpServer.IsAlive)
                {
                    return;
                }

                try
                {
                    _httpServer?.Dispose();
                }
                catch (Exception)
                {
                }

                _httpServer = new SageHttpServer(
                    BuildSageData,
                    _threads,
                    _log,
                    () => _settings.ServerPort.Value,
                    () => _settings.ServerToken.Value);
                _httpServer.Start();
            }
            catch (Exception ex)
            {
                _log.Log($"Sage HTTP server did not start: {ex.Message}");
                _httpServer = null;
            }
        }
    }

    private SageData BuildSageData()
    {
        EnsureCatalogues();

        Human human = null;
        try
        {
            human = Human.LocalHuman;
        }
        catch (Exception)
        {
        }

        var player = new StationeersPlayerContext(human);
        return new SageData
        {
            Items = _catalogueItems,
            Structures = _catalogueStructures,
            Recipes = _catalogueRecipes,
            Stationpedia = _catalogueStationpedia,
            Player = player,
            Worlds = new StationeersWorldContext(),
            Ic = new StationeersIcEditor(_threads, _log),
            Export = new StationeersIcExport(_threads, _log),
            Mips = new StationeersMipsReference(_log),
            Devices = new StationeersDeviceCatalog(_threads, _log),
            Inspector = new StationeersDeviceInspector(_threads, _log),
            Things = new StationeersThingCatalog(_threads, _log),
            Rooms = new StationeersRoomInspector(_threads, _log),
        };
    }

    /// <summary>
    /// Builds the window holder, view, and presenter, keeping the history
    /// model across rebuilds. The holder is DontDestroyOnLoad, but scene
    /// transitions can still drop it, so this runs in Awake, when the world
    /// starts, on every scene load, and before each use. Unity's fake null
    /// counts destroyed objects.
    /// </summary>
    private void EnsureWindow()
    {
        if (_window != null)
        {
            return;
        }

        var holder = new GameObject("SageWindow");
        UnityEngine.Object.DontDestroyOnLoad(holder);
        _window = holder.AddComponent<SageImGuiWindow>();
        _window.Logger = _log;
        _window.WindowToggle = () => _settings.WindowToggle.Value;
        _sage = new SagePresenter(
            _windowModel,
            _window,
            activity => BuildAssistant(activity),
            PlayerName,
            () => AssistantName,
            _log,
            () => _settings.ShowThinking.Value,
            () => _settings.ShowToolCalls.Value,
            () => _settings.ShowToolResults.Value,
            () => _settings.ContextMessages.Value > 0 ? Conversation : null,
            _threads.Run);
        _mentionResponder = null;
    }

    /// <summary>
    /// Called for every chat message displayed on this machine. Mentions
    /// are always answered; other messages are answered directly in local
    /// games and empty servers. The question is echoed to the Sage window
    /// and answered there through the window presenter. Runs on the Unity
    /// main thread, so the game-data snapshot is safe to build. Ignored
    /// while another turn is running.
    /// </summary>
    /// <summary>Harmony entry: chat messages displayed on this machine.</summary>
    public static void OnChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        _instance?.HandleChatMessage(message);
    }

    /// <summary>
    /// Whether every chat message is answered directly: local games and
    /// empty servers have no audience to disturb, so no mention prefix is
    /// needed. Fails closed to mentions only.
    /// </summary>
    private bool AnswersDirectly()
    {
        try
        {
            if (!Assets.Scripts.Networking.NetworkManager.IsClient)
            {
                return true;
            }

            return Assets.Scripts.Networking.NetworkManager.TotalPlayersInGame <= 1;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void HandleChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        EnsureWindow();
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

        var mentioned = Mention.TryParse(_settings.MentionTrigger.Value, message.ChatText, out var prompt);
        if (!mentioned)
        {
            if (!AnswersDirectly())
            {
                return;
            }

            prompt = message.ChatText.Trim();
            _log.Log($"direct chat asking: {Truncate(prompt, 80)}");
        }
        else
        {
            _log.Log($"mention matched, asking: {Truncate(prompt, 80)}");
        }
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
            new FanoutChatSink(_sage, new ChatMessageSink(_threads, _log, () => AssistantName)),
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
            new WriteIcTool(new WriteIcCommand(new StationeersIcEditor(_threads, _log))),
            new ReadIcTool(new ReadIcCommand(new StationeersIcEditor(_threads, _log))),
            new ExportIcTool(new ExportIcCommand(new StationeersIcExport(_threads, _log))),
            new MipsReferenceTool(new ReadMipsCommand(new StationeersMipsReference(_log))),
            new SearchDevicesTool(new SearchDevicesCommand(new StationeersDeviceCatalog(_threads, _log))),
            new DescribeDeviceTool(new DescribeDeviceCommand(new StationeersDeviceInspector(_threads, _log), new StationeersDeviceCatalog(_threads, _log))),
            new SearchThingsTool(new SearchThingsCommand(new StationeersThingCatalog(_threads, _log), playerContext)),
            new ReadRoomTool(new ReadRoomCommand(new StationeersRoomInspector(_threads, _log), playerContext)),
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
