using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Reqnroll;
using Shouldly;
using StationeersAi.Test.Support;
using Caev.Tools;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class ToolsSteps
{
    private readonly ScenarioContext _context;

    public ToolsSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [When("I call the {string} tool with query {string}")]
    public void WhenICallTheToolWithQuery(string tool, string query)
    {
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new ToolCall("call_1", tool, Arguments("query", query)));
    }

    [When("I call the {string} tool with key {string}")]
    public void WhenICallTheToolWithKey(string tool, string key)
    {
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new ToolCall("call_1", tool, Arguments("key", key)));
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

    [Then("the tool result is:")]
    public void ThenTheToolResultIs(string expected)
    {
        World.ToolResult.Content.ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the tool fails with:")]
    public void ThenTheToolFailsWithBlock(string expected)
    {
        World.ToolResult.IsSuccess.ShouldBeFalse();
        World.ToolResult.Content.ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the tool result contains {string}")]
    public void ThenTheToolResultContainsInline(string expected)
    {
        World.ToolResult.Content.ShouldContain(expected);
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

    [Then("the recipe {string} is made at {string}")]
    public void ThenTheRecipeIsMadeAt(string name, string station)
    {
        RecipeResults().Single(recipe => recipe.Name == name).Station.ShouldBe(station);
    }

    [Then("the stationpedia result titles are {string}")]
    public void ThenTheStationpediaResultTitlesAre(string expected)
    {
        var titles = World.ToolResult.ShouldBeOfType<ToolSuccess<SearchStationpediaResult>>().Value.Entries;
        string.Join(", ", titles.Select(entry => entry.Title)).ShouldBe(expected);
    }

    [Then("the stationpedia page {string} reads:")]
    public void ThenTheStationpediaPageReads(string key, string expected)
    {
        var page = World.ToolResult.ShouldBeOfType<ToolSuccess<ReadStationpediaResult>>().Value;
        page.Entry.Key.ShouldBe(key);
        $"{page.Entry.Title}\n{page.Entry.Text}".Trim().ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the inventory result is:")]
    public void ThenTheInventoryResultIs(Table table)
    {
        var stacks = World.ToolResult.ShouldBeOfType<ToolSuccess<SearchInventoryResult>>().Value.Stacks;
        stacks.Count.ShouldBe(table.Rows.Count);

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            stacks[i].Name.ShouldBe(row["Name"]);
            stacks[i].Quantity.ShouldBe(row["Quantity"]);
            stacks[i].Slot.ShouldBe(row["Slot"]);
            if (row.ContainsKey("Hand"))
            {
                (stacks[i].Hand ?? string.Empty).ShouldBe(row["Hand"]);
            }
        }
    }

    [Then("the status result is:")]
    public void ThenTheStatusResultIs(Table table)
    {
        var status = World.ToolResult.ShouldBeOfType<ToolSuccess<DescribePlayerResult>>().Value.Status;
        var row = table.Rows.Single();

        status.X.ShouldBe(float.Parse(row["X"], CultureInfo.InvariantCulture));
        status.Y.ShouldBe(float.Parse(row["Y"], CultureInfo.InvariantCulture));
        status.Z.ShouldBe(float.Parse(row["Z"], CultureInfo.InvariantCulture));
        status.World.ShouldBe(row["World"]);
    }

    private IReadOnlyList<Entity.Item> ItemResults()
    {
        return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchItemsResult>>().Value.Items;
    }

    private IReadOnlyList<Entity.Recipe> RecipeResults()
    {
        return World.ToolResult.ShouldBeOfType<ToolSuccess<SearchRecipesResult>>().Value.Recipes;
    }

    private static ToolArguments Arguments(string name, string value)
    {
        return new ToolArguments(new Dictionary<string, string> { [name] = value });
    }
}
