using OdinsAi.Tool;
using OdinsAi.Repository;
using OdinsAi.Test.Fakes;

namespace OdinsAi.Test.Support
{
    /// <summary>
    /// Shared state for a scenario. Built once per scenario by <see cref="Hooks"/>
    /// and shared by all step bindings.
    /// </summary>
    public sealed class World
    {
        public InMemoryRecipeRepository Recipes { get; } = new InMemoryRecipeRepository();

        public FakeAiClient Ai { get; set; }

        public ToolResult ToolResult { get; set; }

        public string Reply { get; set; }
    }
}