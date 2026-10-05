using System;
using System.Threading.Tasks;
using Caev;
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
public sealed class World : IDisposable
{
    public InMemoryItemRepository Items { get; } = new InMemoryItemRepository(Array.Empty<Item>());

    public InMemoryStructureRepository Structures { get; } = new InMemoryStructureRepository(Array.Empty<Structure>());

    public InMemoryRecipeRepository Recipes { get; } = new InMemoryRecipeRepository(Array.Empty<Recipe>());

    public InMemoryStationpediaRepository Stationpedia { get; } = new InMemoryStationpediaRepository(Array.Empty<StationpediaEntry>());

    public InMemoryPlayerContext Player { get; } = new InMemoryPlayerContext();

    public InMemoryWorldContext Worlds { get; } = new InMemoryWorldContext();

    public InMemoryIcEditor Ic { get; } = new InMemoryIcEditor();

    public InMemoryIcExport Export { get; } = new InMemoryIcExport();

    public InMemoryIcHousing Housing { get; } = new InMemoryIcHousing();

    public InMemoryMipsReference Mips { get; } = new InMemoryMipsReference();

    public InMemoryDeviceCatalog Devices { get; } = new InMemoryDeviceCatalog();

    public InMemoryDeviceInspector Inspector { get; } = new InMemoryDeviceInspector();

    public InMemoryThingCatalog Things { get; } = new InMemoryThingCatalog();

    public InMemoryThingSlots ThingSlots { get; } = new InMemoryThingSlots();

    public InMemoryRoomInspector RoomInspector { get; } = new InMemoryRoomInspector();

    public FakeAiClient Ai { get; set; }

    public RecordingChatSink Chat { get; } = new RecordingChatSink();

    public RecordingLogger Log { get; } = new RecordingLogger();

    public string PlayerName { get; set; } = "Tester";

    public bool GameDataLoaded { get; set; } = true;

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

    public RecordingActivity Activity { get; set; } = new RecordingActivity();

    public ToolResult ToolResult { get; set; }

    public string Reply { get; set; }

    public void Dispose()
    {
    }
}
