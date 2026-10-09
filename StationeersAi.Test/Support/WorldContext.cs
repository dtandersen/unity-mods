using System;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Core;
using StationeersAi.Entity;
using StationeersAi.Repository;
using StationeersAi.Test.Fakes;
using Caev.Tools;
using StationeersAi.Window;
using StationeersAi.Ai;

namespace StationeersAi.Test.Support;

/// <summary>
/// Shared state for a scenario. Built once per scenario by <see cref="Hooks"/>
/// and shared by all step bindings.
/// </summary>
public sealed class WorldContext : IDisposable, AgentSettings
{
    public InMemoryItemRepository Items { get; } = new InMemoryItemRepository(Array.Empty<Item>());

    public InMemoryStructureRepository Structures { get; } = new InMemoryStructureRepository(Array.Empty<Structure>());

    public InMemoryRecipeRepository Recipes { get; } = new InMemoryRecipeRepository(Array.Empty<Recipe>());

    public InMemoryStationpediaRepository Stationpedia { get; } = new InMemoryStationpediaRepository(Array.Empty<StationpediaEntry>());

    public InMemoryPlayerContext Player { get; } = new InMemoryPlayerContext();

    public InMemoryWorldContext Worlds { get; } = new InMemoryWorldContext();

    public InMemoryIcDevices IcDevices { get; } = new InMemoryIcDevices();

    /// <summary>The one computer lookups resolve to.</summary>
    public InMemoryComputer Computer => IcDevices.Computer;

    /// <summary>The one housing lookups resolve to.</summary>
    public InMemoryHousing Housing => IcDevices.Housing;

    public InMemoryMipsReference Mips { get; } = new InMemoryMipsReference();

    public InMemoryDeviceCatalog Devices { get; } = new InMemoryDeviceCatalog();

    public InMemoryDeviceInspector Inspector { get; } = new InMemoryDeviceInspector();

    public InMemoryThingCatalog Things { get; } = new InMemoryThingCatalog();

    public InMemoryThingSlots ThingSlots { get; } = new InMemoryThingSlots();

    public InMemoryRoomInspector RoomInspector { get; } = new InMemoryRoomInspector();

    /// <summary>The one model this scenario talks to, standing by before the gateway is built.</summary>
    public FakeAiClient Ai { get; } = new FakeAiClient();

    public RecordingChatSink Chat { get; } = new RecordingChatSink();

    public RecordingLogger Log { get; } = new RecordingLogger();

    public string PlayerName { get; set; } = "Tester";

    public string AssistantName { get; set; } = "Sage";

    public TimeSpan ThinkingElapsed { get; set; }

    public WindowModel Window { get; set; } = new WindowModel(200);

    public FakeSageView WindowView { get; set; }

    public SagePresenter WindowPresenter { get; set; }

    public bool ShowThinking { get; set; }

    public bool ShowToolCalls { get; set; }

    public bool ShowToolResults { get; set; }

    public MentionResponder MentionResponder { get; set; }

    public string MentionTrigger { get; set; } = "!sage";

    public Task AskTask { get; set; }

    public string ConsoleOutput { get; set; }

    public Conversation Conversation { get; set; } = new Conversation();

    /// <summary>Instructions each turn sends. Null or blank sends none.</summary>
    public string SystemPrompt { get; set; }

    /// <summary>Tool round-trips each turn may take.</summary>
    public int MaxIterations { get; set; } = 5;

    /// <summary>A turn built from this scenario's prompt, history, and settings.</summary>
    public TurnContext Turn(string prompt, Conversation conversation = null)
        => new TurnContext(prompt, conversation, SystemPrompt, MaxIterations);

    /// <summary>Where this scenario's turns report: reasoning, tool traffic, answer.</summary>
    public AgentEvents Events { get; } = new AgentEvents();

    /// <summary>
    /// The one gateway for this scenario, with answers posted to <see cref="Chat"/>.
    /// Specs that ask an agent ask this, the way the mod asks its one gateway.
    /// </summary>
    public AgentGateway Gateway => _gateway ??= BuildGateway();

    private AgentGateway _gateway;

    private AgentGateway BuildGateway()
    {
        var gateway = TestComposition.Gateway(this);
        _answers = new ChatAnswerSink(gateway, Chat, AssistantName);
        return gateway;
    }

    private ChatAnswerSink _answers;

    /// <summary>What the turns reported through <see cref="Events"/>.</summary>
    public EventRecorder Recorder { get; }

    public WorldContext()
    {
        // Subscribe before any turn runs, so nothing reported is missed.
        Recorder = new EventRecorder(Events);
    }

    public ToolResult ToolResult { get; set; }

    /// <summary>What the last turn answered, read from the events it reported.</summary>
    public string Reply => Recorder.Answers.Count > 0 ? Recorder.Answers[Recorder.Answers.Count - 1] : string.Empty;

    public void Dispose()
    {
    }
}
