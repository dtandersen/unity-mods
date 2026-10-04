using System;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using Caev;
using Caev.OpenAi;
using Caev.Tools;
using StationeersAi.Command;
using StationeersAi.Formatting;
using StationeersAi.Game.Ai;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Server;
using StationeersAi.Tool;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Builds the assistant from live game data and owns the shared
/// conversations. The shell keeps one for the session; the presenter asks
/// it for an assistant each turn and the mod asks it for server data.
/// </summary>
public sealed class SageAssistantFactory
{
    private readonly SageConfig _settings;
    private readonly string _configPath;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;

    private readonly Dictionary<string, Conversation> _mentionConversations = new Dictionary<string, Conversation>();

    // World data does not depend on the player, so it is built once and
    // reused until the prefab catalogue changes size. Set by
    // EnsureCatalogues before any read.
    private IItemRepository _catalogueItems = null!;
    private IStructureRepository _catalogueStructures = null!;
    private IRecipeRepository _catalogueRecipes = null!;
    private IStationpediaRepository _catalogueStationpedia = null!;
    private int _cataloguePrefabCount = -1;

    private Conversation? _conversation;

    public SageAssistantFactory(SageConfig settings, string configPath, MainThreadExecutor threads, Logger log)
    {
        _settings = settings;
        _configPath = configPath;
        _threads = threads;
        _log = log;
    }

    /// <summary>
    /// Display name for the assistant. Configurable via the
    /// <c>General/Name</c> entry; falls back to "Sage" when blank.
    /// </summary>
    public string AssistantName => ChatNaming.Resolve(_settings.AiName?.Value);

    private Conversation Conversation => _conversation ??= new Conversation(_settings.ContextMessages.Value);

    /// <summary>Shared console history. Null when history is off.</summary>
    public Conversation? ConsoleConversation => _settings.ContextMessages.Value > 0 ? Conversation : null;

    /// <summary>One chat session per player, capped like the console history.</summary>
    public Conversation? MentionConversation(string? player)
    {
        if (_settings.ContextMessages.Value <= 0)
        {
            return null;
        }

        if (player is null || string.IsNullOrWhiteSpace(player))
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

    public string? PlayerName()
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

    public SageData BuildSageData()
    {
        EnsureCatalogues();

        Human? human = null;
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

    public AiAssistant? BuildAssistant(IAssistantActivity? activity)
    {
        var prefabs = Prefab.AllPrefabs;
        if (prefabs == null || prefabs.Count == 0)
        {
            return null;
        }

        EnsureCatalogues();

        Human? human = null;
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
