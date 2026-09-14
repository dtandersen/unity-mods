using System;
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

        public FakeAiClient Ai { get; set; }

        public Conversation Conversation { get; set; } = new Conversation();

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