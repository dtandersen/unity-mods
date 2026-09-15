using System;
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
            var itemIndex = new ItemIndex(world.Items);

            return new ToolHandler(new ITool[]
            {
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(world.Recipes))),
                new SearchUpgradesTool(new SearchUpgradesCommand(new RecipeIndex(world.Recipes))),
                new SearchFoodTool(new SearchFoodCommand(itemIndex)),
                new SearchItemsTool(new SearchItemsCommand(itemIndex)),
                new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(world.Inventory))),
                new SearchStorageTool(new SearchStorageCommand(new StorageIndex(world.Storage))),
                new SearchDungeonsTool(new SearchDungeonsCommand(new DungeonIndex(world.Dungeons))),
                new SearchCreaturesTool(new SearchCreaturesCommand(new CreatureIndex(world.Creatures))),
                new SearchResourcesTool(new SearchResourcesCommand(new ResourceIndex(world.Resources))),
                new SearchCompendiumTool(new SearchCompendiumCommand(new CompendiumIndex(world.Compendium))),
                new ReadCompendiumTool(new ReadCompendiumCommand(new CompendiumIndex(world.Compendium))),
                new GetLocationTool(new GetLocationCommand(world.Location)),
                new ListBossesTool(new GetBossStatusCommand(world.Bosses)),
            });
        }

        public static AiAssistant Assistant(World world)
        {
            return new AiAssistant(world.Ai, Tools(world));
        }

        public static AiAssistant Assistant(World world, IAssistantActivity activity)
        {
            return new AiAssistant(world.Ai, Tools(world), null, 5, null, activity);
        }

        public static MentionResponder Responder(World world)
        {
            return new MentionResponder(
                world.Chat,
                () => Assistant(world),
                world.AssistantName);
        }

        public static ChatPresenter Presenter(World world)
        {
            return new ChatPresenter(
                world.Chat,
                activity => world.GameDataLoaded ? Assistant(world, activity) : null,
                () => world.PlayerName,
                world.AssistantName,
                showThinking: world.ShowThinking,
                showToolCalls: world.ShowToolCalls,
                showToolResults: world.ShowToolResults,
                clock: world.Clock);
        }
    }
}