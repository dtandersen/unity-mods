using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Caev;
using Caev.OpenAi;
using Caev.Tools;
using StationeersAi.Formatting;
using StationeersAi.Game.Ai;
using StationeersAi.Server;
using StationeersAi.Tool;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Builds the assistant from the factory's commands and owns the one shared
/// conversation. The shell keeps one for the session; the presenter asks
/// it for an assistant each turn and the mod asks it for server data.
/// </summary>
public sealed class SageAssistantFactory
{
    private readonly SageConfig _settings;
    private readonly string _configPath;
    private readonly CommandFactory _commands;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;

    private Conversation? _conversation;

    public SageAssistantFactory(SageConfig settings, string configPath, CommandFactory commands, MainThreadExecutor threads, Logger log)
    {
        _settings = settings;
        _configPath = configPath;
        _commands = commands;
        _threads = threads;
        _log = log;
    }

    /// <summary>
    /// Display name for the assistant. Configurable via the
    /// <c>General/Name</c> entry; falls back to "Sage" when blank.
    /// </summary>
    public string AssistantName => ChatNaming.Resolve(_settings.AiName?.Value);

    private Conversation Conversation
    {
        get
        {
            var limit = _settings.ContextBytes.Value;
            if (_conversation is null)
            {
                _conversation = new Conversation(limit);
            }
            else if (_conversation.MaxBytes != limit)
            {
                var kept = _conversation.Messages;
                _conversation = new Conversation(limit);
                _conversation.Replace(kept);
            }

            return _conversation;
        }
    }

    /// <summary>Shared console history. Null when history is off.</summary>
    public Conversation? ConsoleConversation => _settings.ContextBytes.Value > 0 ? Conversation : null;

    /// <summary>The one shared history for chat mentions and the console. Null when history is off.</summary>
    public Conversation? MentionConversation()
    {
        return _settings.ContextBytes.Value > 0 ? Conversation : null;
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

    public SageCommands Commands => _commands.Commands();

    public AgentGateway? BuildAssistant(AgentEvents events)
    {
        var prefabs = Prefab.AllPrefabs;
        if (prefabs == null || prefabs.Count == 0)
        {
            return null;
        }

        var commands = _commands.Commands();
        var tools = new ToolHandler(new ITool[]
        {
            new SearchItemsTool(commands.SearchItems()),
            new SearchStructuresTool(commands.SearchStructures()),
            new SearchRecipesTool(commands.SearchRecipes()),
            new SearchStationpediaTool(commands.SearchStationpedia()),
            new ReadStationpediaTool(commands.ReadStationpedia()),
            new SearchInventoryTool(commands.SearchInventory()),
            new DescribePlayerTool(commands.DescribePlayer()),
            new GetWorldTool(commands.GetWorld()),
            new WriteIcTool(commands.WriteComputer()),
            new ReadIcTool(commands.ReadComputer(), commands.ReadHousing()),
            new ExportIcTool(commands.ExportChip()),
            new ImportIcTool(commands.ImportChip()),
            new SelectIcTool(commands.SelectHousing()),
            new ListInstructionsTool(commands.ListInstructions()),
            new SearchDevicesTool(commands.SearchDevices()),
            new DescribeDeviceTool(commands.DescribeDevice()),
            new SearchThingsTool(commands.SearchThings()),
            new ReadSlotsTool(commands.ReadThingSlots()),
            new ReadRoomTool(commands.ReadRoom()),
        });

        var client = new OpenAiChatClient(
            new UnityWebRequestTransport(_settings.TimeoutSeconds.Value),
            new OpenAiOptions(_settings.Endpoint.Value, _settings.ApiKey.Value, _settings.Model.Value, _settings.ReasoningEffort.Value));

        return new Agent(client, tools, SystemPromptText(), _settings.MaxIterations.Value, _log, events);
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
            _log.Info($"SYSTEM.md unreadable, using configured prompt: {ex.Message}");
        }

        return _settings.SystemPrompt.Value;
    }
}
