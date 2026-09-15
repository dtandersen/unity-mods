using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OdinsAi.Ai;
using OdinsAi.Repository;
using OdinsAi.Test.Fakes;
using OdinsAi.Tool;

namespace OdinsAi.Test.Support
{
    /// <summary>
    /// Shared state for a scenario. Built once per scenario by <see cref="Hooks"/>
    /// and shared by all step bindings.
    /// </summary>
    public sealed class World : IDisposable
    {
        public InMemoryRecipeRepository Recipes { get; } = new InMemoryRecipeRepository();

        public InMemoryItemRepository Items { get; } = new InMemoryItemRepository();

        public InMemoryInventoryRepository Inventory { get; } = new InMemoryInventoryRepository();

        public InMemoryStorageRepository Storage { get; } = new InMemoryStorageRepository();

        public InMemoryDungeonRepository Dungeons { get; } = new InMemoryDungeonRepository();

        public InMemoryCreatureRepository Creatures { get; } = new InMemoryCreatureRepository();

        public InMemoryResourceRepository Resources { get; } = new InMemoryResourceRepository();

        public InMemoryCompendiumRepository Compendium { get; } = new InMemoryCompendiumRepository();

        public InMemoryLocationRepository Location { get; } = new InMemoryLocationRepository();

        public InMemoryBossRepository Bosses { get; } = new InMemoryBossRepository();

        public FakeAiClient Ai { get; set; }

        public RecordingChatSink Chat { get; } = new RecordingChatSink();

        public string PlayerName { get; set; } = "Tester";

        public bool GameDataLoaded { get; set; } = true;

        public string AssistantName { get; set; } = "Odin";

        public FakeClock Clock { get; } = new FakeClock();

        public bool ShowThinking { get; set; }

        public bool ShowToolCalls { get; set; }

        public bool ShowToolResults { get; set; }

        public ChatPresenter Presenter { get; set; }

        public MentionResponder MentionResponder { get; set; }

        public string MentionTrigger { get; set; } = "!odin";

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