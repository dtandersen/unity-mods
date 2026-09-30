using System;
using System.Threading.Tasks;
using StationeersAi.Ai;
using StationeersAi.Repository;
using StationeersAi.Test.Fakes;
using StationeersAi.Tool;

namespace StationeersAi.Test.Support
{
    /// <summary>
    /// Shared state for a scenario. Built once per scenario by <see cref="Hooks"/>
    /// and shared by all step bindings.
    /// </summary>
    public sealed class World : IDisposable
    {
        public InMemoryItemRepository Items { get; } = new InMemoryItemRepository();

        public InMemoryStructureRepository Structures { get; } = new InMemoryStructureRepository();

        public InMemoryRecipeRepository Recipes { get; } = new InMemoryRecipeRepository();

        public InMemoryStationpediaRepository Stationpedia { get; } = new InMemoryStationpediaRepository();

        public InMemoryPlayerContext Player { get; } = new InMemoryPlayerContext();

        public FakeAiClient Ai { get; set; }

        public RecordingChatSink Chat { get; } = new RecordingChatSink();

        public RecordingLogger Log { get; } = new RecordingLogger();

        public string PlayerName { get; set; } = "Tester";

        public bool GameDataLoaded { get; set; } = true;

        public string AssistantName { get; set; } = "Sage";

        public FakeClock Clock { get; } = new FakeClock();

        public bool ShowThinking { get; set; }

        public bool ShowToolCalls { get; set; }

        public bool ShowToolResults { get; set; }

        public ChatPresenter Presenter { get; set; }

        public MentionResponder MentionResponder { get; set; }

        public string MentionTrigger { get; set; } = "!sage";

        public Task AskTask { get; set; }

        public Conversation Conversation { get; set; } = new Conversation();

        public RecordingActivity Activity { get; set; } = new RecordingActivity();

        public ToolResult ToolResult { get; set; }

        public string Reply { get; set; }

        public FakeOpenAiServer Server { get; set; }

        public AiCompletion Completion { get; set; }

        public AiClientException CompletionError { get; set; }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}
