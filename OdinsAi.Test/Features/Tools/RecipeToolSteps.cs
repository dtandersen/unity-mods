using System.Collections.Generic;
using System.Linq;
using OdinsAi.Test.Support;
using OdinsAi.Tool;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Tools
{
    [Binding]
    public sealed class RecipeToolSteps
    {
        private readonly ScenarioContext _context;

        public RecipeToolSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [When("I call the {string} tool with query {string}")]
        public void WhenICallTheToolWithQuery(string tool, string query)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall("call_1", tool, Arguments("query", query)));
        }

        [When("I call the {string} tool without arguments")]
        public void WhenICallTheToolWithoutArguments(string tool)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall("call_1", tool, ToolArguments.Empty));
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

        [Then("the tool result contains a recipe {string}")]
        public void ThenTheToolResultContainsARecipe(string name)
        {
            Recipes().Any(recipe => recipe.Name == name).ShouldBeTrue(
                $"expected '{name}' in: {string.Join(", ", Recipes().Select(r => r.Name))}");
        }

        [Then("the tool result does not contain a recipe {string}")]
        public void ThenTheToolResultDoesNotContainARecipe(string name)
        {
            Recipes().Any(recipe => recipe.Name == name).ShouldBeFalse(
                $"did not expect '{name}'");
        }

        [Then("the tool result contains a recipe {string} with {int} ingredients")]
        public void ThenTheToolResultContainsARecipeWithIngredients(string name, int count)
        {
            var recipe = Recipes().Single(r => r.Name == name);
            recipe.Ingredients.Count.ShouldBe(count);
        }

        private IReadOnlyList<RecipeSummary> Recipes()
        {
            return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchRecipesResult>>().Value.Recipes;
        }

        private static ToolArguments Arguments(string name, string value)
        {
            return new ToolArguments(new Dictionary<string, string> { [name] = value });
        }
    }
}