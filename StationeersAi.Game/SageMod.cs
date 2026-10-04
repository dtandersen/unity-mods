using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using Caev;
using StationeersAi.Command;
using StationeersAi.Formatting;
using StationeersAi.Game.Ai;
using StationeersAi.Game.Server;
using StationeersAi.Query;
using StationeersAi.Repository;
using Caev.Tools;
using StationeersAi.Window;
using Caev.OpenAi;
using StationeersAi.Ai;
using StationeersAi.Server;
using StationeersAi.Tool;

namespace StationeersAi.Game;

/// <summary>
/// The mod itself: every runtime object Sage owns (window, HTTP server,
/// catalogues, conversations). The BepInEx shell only constructs this,
/// forwards lifecycle, and patches. Load builds world-bound objects when a
/// level loads; Unload tears them down cleanly when it unloads.
/// </summary>
public sealed class SageMod
{
    private readonly SageConfig _settings;
    private readonly string _configPath;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;

    private bool _loaded;
    private SagePresenter _sage;
    private MentionResponder _mentionResponder;
    private WindowModel _windowModel = new WindowModel(200);
    private GameObject _holder;
    private SageImGuiWindow _window;
    private SageHttpServer _httpServer;

    private readonly Dictionary<string, Conversation> _mentionConversations = new Dictionary<string, Conversation>();

    // World data does not depend on the player, so it is built once and
    // reused until the prefab catalogue changes size.
    private IItemRepository _catalogueItems;
    private IStructureRepository _catalogueStructures;
    private IRecipeRepository _catalogueRecipes;
    private IStationpediaRepository _catalogueStationpedia;
    private int _cataloguePrefabCount = -1;

    private Conversation _conversation;

    public SageMod(SageConfig settings, string configPath, MainThreadExecutor threads, Logger log)
    {
        _settings = settings;
        _configPath = configPath;
        _threads = threads;
        _log = log;
    }

    public bool IsLoaded => _loaded;

    private Conversation Conversation => _conversation ??= new Conversation(_settings.ContextMessages.Value);

    /// <summary>Builds window and server for the loaded level. No-op when loaded.</summary>
    public void Load()
    {
        if (_loaded)
        {
            return;
        }

        if (!_settings.ModEnabled.Value)
        {
            return;
        }

        EnsureWindow();
        EnsureHttpServer();
        _loaded = true;
        _log.Log("mod loaded");
    }

    /// <summary>Tears down window and server. No-op when not loaded.</summary>
    public void Unload()
    {
        if (!_loaded)
        {
            return;
        }

        _loaded = false;

        try
        {
            _httpServer?.Dispose();
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
        }

        _httpServer = null;

        try
        {
            if (_holder != null)
            {
                UnityEngine.Object.Destroy(_holder);
            }
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
        }

        _holder = null;
        _window = null;
        _sage = null;
        _mentionResponder = null;
        _log.Log("mod unloaded");
    }

    /// <summary>The presenter for the <c>ai</c> console command.</summary>
    public SagePresenter ConsolePresenter()
    {
        return _sage;
    }

    /// <summary>
    /// Display name for the assistant. Configurable via the
    /// <c>General/Name</c> entry; falls back to "Sage" when blank.
    /// </summary>
    public string AssistantName => ChatNaming.Resolve(_settings.AiName?.Value);

    /// <summary>
    /// Called for every chat message displayed on this machine. Mentions
    /// are always answered; other messages are answered directly in local
    /// games and empty servers. The question is echoed to the Sage window
    /// and answered there through the window presenter. Runs on the Unity
    /// main thread, so the game-data snapshot is safe to build. Ignored
    /// while another turn is running.
    /// </summary>
    public void HandleChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        if (!_loaded || message == null)
        {
            return;
        }

        if (!_settings.ModEnabled.Value || !_settings.RespondToMentions.Value)
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

        EnsureWindow();
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

    /// <summary>
    /// Starts the loopback HTTP server for outside tools such as Pi when
    /// <c>Server/Enabled</c> is on. Each request reuses the same live
    /// repositories the chat assistant reads.
    /// </summary>
    private void EnsureHttpServer()
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
            ThingContents = new StationeersThingContents(_threads, _log),
            Rooms = new StationeersRoomInspector(_threads, _log),
        };
    }

    /// <summary>
    /// Builds the window holder, view, and presenter, keeping the history
    /// model across rebuilds. Unity's fake null counts destroyed objects.
    /// </summary>
    private void EnsureWindow()
    {
        if (_window != null)
        {
            return;
        }

        _holder = new GameObject("SageWindow");
        UnityEngine.Object.DontDestroyOnLoad(_holder);
        _window = _holder.AddComponent<SageImGuiWindow>();
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
        _log.Log($"Sage window ready ({_settings.WindowToggle.Value})");
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

    private static string Truncate(string value, int max)
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
            var path = System.IO.Path.Combine(_configPath, "SYSTEM.md");
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
