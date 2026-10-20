using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Query;
using OdinsAi.Test.Support;
using OdinsAi.Tool;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Tools
{
    [Binding]
    public sealed class ToolsSteps
    {
        private readonly ScenarioContext _context;

        public ToolsSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        private IReadOnlyList<string> _itemTypes;

        [When("I list the item types")]
        public void WhenIListTheItemTypes()
        {
            _itemTypes = new ItemIndex(World.Items).GetTypes();
        }

        [Then("the item types are {string}")]
        public void ThenTheItemTypesAre(string expected)
        {
            string.Join(", ", _itemTypes).ShouldBe(expected);
        }

        [When("I call the {string} tool with query {string}")]
        public void WhenICallTheToolWithQuery(string tool, string query)
        {
            World.ToolResult = TestComposition.Tools(World)
                .Handle(new ToolCall("call_1", tool, Arguments("query", query)));
        }

        [When("I call the {string} tool with type {string}")]
        public void WhenICallTheToolWithType(string tool, string type)
        {
            World.ToolResult = TestComposition.Tools(World)
                .Handle(new ToolCall("call_1", tool, Arguments("type", type)));
        }

        [When("I call the {string} tool with topic {string}")]
        public void WhenICallTheToolWithTopic(string tool, string topic)
        {
            World.ToolResult = TestComposition.Tools(World)
                .Handle(new ToolCall("call_1", tool, Arguments("topic", topic)));
        }

        [When("I call the {string} tool without arguments")]
        public void WhenICallTheToolWithoutArguments(string tool)
        {
            World.ToolResult = TestComposition.Tools(World)
                .Handle(new ToolCall("call_1", tool, ToolArguments.Empty));
        }

        [When("I call the {string} tool")]
        public void WhenICallTheTool(string tool)
        {
            World.ToolResult = TestComposition.Tools(World)
                .Handle(new ToolCall("call_1", tool, ToolArguments.Empty));
        }

        [Then("the tool succeeds")]
        public void ThenTheToolSucceeds()
        {
            World.ToolResult.IsSuccess.ShouldBeTrue($"tool failed: {World.ToolResult.Content}");
        }

        [Then("the tool fails with {string}")]
        public void ThenTheToolFailsWith(string message)
        {
            World.ToolResult.IsSuccess.ShouldBeFalse();
            World.ToolResult.Content.ShouldBe(message);
        }

        [Then("the tool result contains:")]
        public void ThenTheToolResultContains(string expected)
        {
            World.ToolResult.Content.ShouldContain(expected.Replace("\r\n", "\n").Trim());
        }

        [Then("the compendium result topics are {string}")]
        public void ThenTheCompendiumResultTopicsAre(string expected)
        {
            var topics = World.ToolResult.ShouldBeOfType<ToolSuccess<SearchCompendiumResult>>().Value.Entries;
            string.Join(", ", topics.Select(entry => entry.Topic)).ShouldBe(expected);
        }

        [Then("the compendium page {string} reads:")]
        public void ThenTheCompendiumPageReads(string topic, string expected)
        {
            var page = World.ToolResult.ShouldBeOfType<ToolSuccess<ReadCompendiumResult>>().Value;
            page.Topic.ShouldBe(topic);
            page.Text.ShouldBe(expected.Replace("\r\n", "\n").Trim());
        }

        [Then("the location result is:")]
        public void ThenTheLocationResultIs(Table table)
        {
            var location = World.ToolResult.ShouldBeOfType<ToolSuccess<LocationResult>>().Value;
            var row = table.Rows.Single();

            location.X.ShouldBe(float.Parse(row["X"], CultureInfo.InvariantCulture));
            location.Y.ShouldBe(float.Parse(row["Y"], CultureInfo.InvariantCulture));
            location.Z.ShouldBe(float.Parse(row["Z"], CultureInfo.InvariantCulture));
            location.Biome.ShouldBe(row["Biome"]);
        }

        [Then("the boss result is:")]
        public void ThenTheBossResultIs(Table table)
        {
            var bosses = World.ToolResult.ShouldBeOfType<ToolSuccess<ListBossesResult>>().Value.Bosses;
            bosses.Count.ShouldBe(table.Rows.Count);

            for (var i = 0; i < table.Rows.Count; i++)
            {
                bosses[i].Name.ShouldBe(table.Rows[i]["Boss"]);
                bosses[i].Defeated.ShouldBe(
                    table.Rows[i]["Defeated"].Equals("Yes", StringComparison.OrdinalIgnoreCase));
            }
        }

        [Then("the upgrade result is:")]
        public void ThenTheUpgradeResultIs(Table table)
        {
            var upgrades = World.ToolResult.ShouldBeOfType<ToolSuccess<SearchUpgradesResult>>().Value.Upgrades;
            upgrades.Count.ShouldBe(table.Rows.Count);

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                upgrades[i].Item.ShouldBe(row["Item"]);
                upgrades[i].Quality.ShouldBe(int.Parse(row["Quality"]));
                upgrades[i].Station.ShouldBe(row["Station"]);
                upgrades[i].StationLevel.ShouldBe(int.Parse(row["StationLevel"]));
                upgrades[i].Unlocked.ShouldBe(row["Unlocked"].Equals("Yes", StringComparison.OrdinalIgnoreCase));
                string.Join("; ", upgrades[i].Ingredients.Select(ingredient => $"{ingredient.ItemId} x{ingredient.Amount}"))
                    .ShouldBe(row["Ingredients"]);
            }
        }

        [Then("the item result is:")]
        public void ThenTheItemResultIs(Table table)
        {
            var actual = ItemResults().ToDictionary(item => item.Name, item => item.Type);
            actual.Count.ShouldBe(table.Rows.Count);

            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                actual.ContainsKey(name).ShouldBeTrue($"expected '{name}' in the result");
                actual[name].ShouldBe(row["Type"]);
            }
        }

        [Then("the creature result is:")]
        public void ThenTheCreatureResultIs(Table table)
        {
            var actual = CreatureResults().ToDictionary(creature => creature.Name, creature => creature);
            actual.Count.ShouldBe(table.Rows.Count);

            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                actual.ContainsKey(name).ShouldBeTrue($"expected '{name}' in the result");
                actual[name].Distance.ShouldBe(float.Parse(row["Distance"], CultureInfo.InvariantCulture));
                actual[name].Direction.ShouldBe(row["Direction"]);
            }
        }

        [Then("the resource result is:")]
        public void ThenTheResourceResultIs(Table table)
        {
            var actual = ResourceResults().ToDictionary(resource => resource.Name, resource => resource);
            actual.Count.ShouldBe(table.Rows.Count);

            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                actual.ContainsKey(name).ShouldBeTrue($"expected '{name}' in the result");
                actual[name].Type.ShouldBe(row["Type"]);
                actual[name].Distance.ShouldBe(float.Parse(row["Distance"], CultureInfo.InvariantCulture));
                actual[name].Direction.ShouldBe(row["Direction"]);
            }
        }

        [Then("the dungeon result is:")]
        public void ThenTheDungeonResultIs(Table table)
        {
            var actual = DungeonResults().ToDictionary(dungeon => dungeon.Name, dungeon => dungeon);
            actual.Count.ShouldBe(table.Rows.Count);

            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                actual.ContainsKey(name).ShouldBeTrue($"expected '{name}' in the result");
                actual[name].Distance.ShouldBe(float.Parse(row["Distance"], CultureInfo.InvariantCulture));
                actual[name].Direction.ShouldBe(row["Direction"]);
            }
        }

        [Then("the tool result contains a recipe {string}")]
        public void ThenTheToolResultContainsARecipe(string name)
        {
            RecipeResults().Any(recipe => recipe.Name == name).ShouldBeTrue(
                $"expected '{name}' in: {string.Join(", ", RecipeResults().Select(r => r.Name))}");
        }

        [Then("the recipe {string} is unlocked")]
        public void ThenTheRecipeIsUnlocked(string name)
        {
            RecipeResults().Single(recipe => recipe.Name == name).Unlocked.ShouldBeTrue();
        }

        [Then("the recipe {string} is locked")]
        public void ThenTheRecipeIsLocked(string name)
        {
            RecipeResults().Single(recipe => recipe.Name == name).Unlocked.ShouldBeFalse();
        }

        [Then("the tool result does not contain a recipe {string}")]
        public void ThenTheToolResultDoesNotContainARecipe(string name)
        {
            RecipeResults().Any(recipe => recipe.Name == name).ShouldBeFalse($"did not expect '{name}'");
        }

        [Then("the tool result contains a recipe {string} with {int} ingredients")]
        public void ThenTheToolResultContainsARecipeWithIngredients(string name, int count)
        {
            RecipeResults().Single(recipe => recipe.Name == name).Ingredients.Count.ShouldBe(count);
        }

        [Then("the tool result contains food {string} with {float} health, {float} stamina and {float} eitr")]
        public void ThenTheToolResultContainsFood(string name, float health, float stamina, float eitr)
        {
            var food = FoodResults().Single(f => f.Name == name);
            food.Health.ShouldBe(health);
            food.Stamina.ShouldBe(stamina);
            food.Eitr.ShouldBe(eitr);
        }

        [Then("the tool result does not contain food {string}")]
        public void ThenTheToolResultDoesNotContainFood(string name)
        {
            FoodResults().Any(food => food.Name == name).ShouldBeFalse($"did not expect '{name}'");
        }

        [Then("the inventory result is:")]
        public void ThenTheInventoryResultIs(Table table)
        {
            AssertStackTotals(InventoryItems(), table);
        }

        [Then("the storage result is:")]
        public void ThenTheStorageResultIs(Table table)
        {
            AssertStackTotals(StorageItems(), table);
        }

        private static void AssertStackTotals(IReadOnlyList<ItemStackSummary> items, Table table)
        {
            var actual = items.ToDictionary(item => item.Name, item => item.Amount);
            actual.Count.ShouldBe(table.Rows.Count);

            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                actual.ContainsKey(name).ShouldBeTrue($"expected '{name}' in the result");
                actual[name].ShouldBe(int.Parse(row["Amount"]));
            }
        }

        private IReadOnlyList<CreatureSummary> CreatureResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchCreaturesResult>>().Value.Creatures;
        }

        private IReadOnlyList<ResourceSummary> ResourceResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchResourcesResult>>().Value.Resources;
        }

        private IReadOnlyList<DungeonSummary> DungeonResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchDungeonsResult>>().Value.Dungeons;
        }

        private IReadOnlyList<ItemSummary> ItemResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchItemsResult>>().Value.Items;
        }

        private IReadOnlyList<RecipeSummary> RecipeResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchRecipesResult>>().Value.Recipes;
        }

        private IReadOnlyList<FoodSummary> FoodResults()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchFoodResult>>().Value.Foods;
        }

        private IReadOnlyList<ItemStackSummary> InventoryItems()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<InventorySearchResult>>().Value.Items;
        }

        private IReadOnlyList<ItemStackSummary> StorageItems()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<StorageSearchResult>>().Value.Items;
        }

        private static ToolArguments Arguments(string name, string value)
        {
            return new ToolArguments(new Dictionary<string, string> { [name] = value });
        }
    }
}