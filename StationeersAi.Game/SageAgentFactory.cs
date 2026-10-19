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
using StationeersAi.Core;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Builds the one gateway for a mod lifecycle and owns the shared
/// conversations. The mod asks for server data from the same commands, and
/// answers what a turn should say about itself: the system prompt and the
/// iteration limit, read fresh each turn so an edit applies without a restart.
/// </summary>
public sealed class SageAgentFactory : AgentSettings
{
    private readonly SageConfig _settings;
    private readonly string _configPath;
    private readonly CommandFactory _commands;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;

    private Conversation? _conversation;

    public SageAgentFactory(SageConfig settings, string configPath, CommandFactory commands, MainThreadExecutor threads, Logger log)
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

    /// <summary>The turn's instructions: <c>SYSTEM.md</c> when it has text, else the configured prompt.</summary>
    public string? SystemPrompt => SystemPromptText();

    /// <summary>Tool round-trips allowed per question (<c>AI/MaxToolIterations</c>).</summary>
    public int MaxIterations => _settings.MaxIterations.Value;

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

    /// <summary>The one shared history, for every surface. Null when history is off.</summary>
    public Conversation? ConversationOrNull => _settings.ContextBytes.Value > 0 ? Conversation : null;

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

    /// <summary>
    /// Builds the gateway: one agent, one channel, every tool. Called on the first
    /// level load, when prefabs and the world are there, so no turn has to check
    /// whether game data exists.
    /// </summary>
    public AgentGateway BuildGateway()
    {
        var commands = _commands.Commands();
        var tools = new ToolHandler(new ITool[]
        {
            new SearchItemsTool(commands.SearchItems()),
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
            new SearchStructuresTool(commands.SearchStructures()),
            new DescribeStructureTool(commands.DescribeStructure()),
            new DescribeThingTool(commands.DescribeThing()),
            new SearchThingsTool(commands.SearchThings()),
            new ReadSlotsTool(commands.ReadThingSlots()),
            new ReadRoomTool(commands.ReadRoom()),
        });

        var client = new OpenAiChatClient(
            new UnityWebRequestTransport(_settings.TimeoutSeconds.Value),
            new OpenAiOptions(_settings.Endpoint.Value, _settings.ApiKey.Value, _settings.Model.Value, _settings.ReasoningEffort.Value));

        return new CaevAgentGateway(client, tools, _log);
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
