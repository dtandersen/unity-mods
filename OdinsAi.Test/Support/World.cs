using System;
using System.Collections.Generic;
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

        public FakeAiClient Ai { get; set; }

        public Conversation Conversation { get; set; } = new Conversation();

        public List<string> Thinking { get; set; } = new List<string>();

        public List<string> ToolOutput { get; set; } = new List<string>();

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