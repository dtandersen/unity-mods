using System.Collections.Generic;
using System.Linq;
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

        [When("I call the {string} tool without arguments")]
        public void WhenICallTheToolWithoutArguments(string tool)
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

        [Then("the tool result contains a recipe {string}")]
        public void ThenTheToolResultContainsARecipe(string name)
        {
            RecipeResults().Any(recipe => recipe.Name == name).ShouldBeTrue(
                $"expected '{name}' in: {string.Join(", ", RecipeResults().Select(r => r.Name))}");
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