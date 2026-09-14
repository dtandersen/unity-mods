using OdinsAi.Ai;
using OdinsAi.Command;
using OdinsAi.Query;
using OdinsAi.Tool;

namespace OdinsAi.Test.Support
{
    /// <summary>
    /// Wires the real core objects for a scenario. Tests exercise public entry
    /// points (<see cref="ToolHandler"/>, <see cref="AiAssistant"/>) rather than
    /// constructing internals inline.
    /// </summary>
    internal static class TestComposition
    {
        public static ToolHandler Tools(World world)
        {
            return new ToolHandler(new ITool[]
            {
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(world.Recipes))),
                new SearchFoodTool(new SearchFoodCommand(new ItemIndex(world.Items))),
            });
        }

        public static AiAssistant Assistant(World world)
        {
            return new AiAssistant(world.Ai, Tools(world));
        }
    }
}