using System;
using System.Globalization;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using Reqnroll;

namespace StationeersAi.Test.Support;

/// <summary>Seeds the in-memory repositories (arrange for the suites).</summary>
[Binding]
public sealed class DatabaseSteps
{
    private readonly ScenarioContext _context;

    public DatabaseSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Given("the item database contains:")]
    public void GivenTheItemDatabaseContains(Table table)
    {
        var world = Hooks.GetWorld(_context);

        foreach (var row in table.Rows)
        {
            world.Items.Add(new Item(row["Name"], row["Name"], row["Type"]));
        }
    }

    [Given("the structure database contains:")]
    public void GivenTheStructureDatabaseContains(Table table)
    {
        var world = Hooks.GetWorld(_context);

        foreach (var row in table.Rows)
        {
            world.Structures.Add(new Structure(
                row["Name"],
                row["Name"],
                row.ContainsKey("Description") ? row["Description"] : null));
        }
    }

    [Given("the recipe database contains {string}")]
    public void GivenTheRecipeDatabaseContains(string name)
    {
        Hooks.GetWorld(_context).Recipes.Add(NewRecipe(name, Array.Empty<Ingredient>(), "Autolathe"));
    }

    [Given("the recipe database contains a {string} recipe with ingredients:")]
    public void GivenTheRecipeDatabaseContainsARecipe(string name, Table table)
    {
        var ingredients = table.Rows
            .Select(row => new Ingredient(row["Ingredient"], Number(row["Amount"])))
            .ToArray();

        Hooks.GetWorld(_context).Recipes.Add(NewRecipe(name, ingredients, "Autolathe"));
    }

    [Given("the recipe database contains a {string} recipe at {string} with ingredients:")]
    public void GivenTheRecipeDatabaseContainsARecipeAt(string name, string station, Table table)
    {
        var ingredients = table.Rows
            .Select(row => new Ingredient(row["Ingredient"], Number(row["Amount"])))
            .ToArray();

        Hooks.GetWorld(_context).Recipes.Add(NewRecipe(name, ingredients, station));
    }

    [Given("the stationpedia contains:")]
    public void GivenTheStationpediaContains(Table table)
    {
        var world = Hooks.GetWorld(_context);

        foreach (var row in table.Rows)
        {
            world.Stationpedia.Add(new StationpediaEntry(
                row["Key"],
                row["Title"],
                row.ContainsKey("Description") ? row["Description"] : null,
                row.ContainsKey("Text") ? row["Text"] : null));
        }
    }

    [Given("my position is {string}, {string}, {string} on {string}")]
    public void GivenMyPositionIs(string x, string y, string z, string world)
    {
        Hooks.GetWorld(_context).Player.Status = new PlayerStatus(
            float.Parse(x, CultureInfo.InvariantCulture),
            float.Parse(y, CultureInfo.InvariantCulture),
            float.Parse(z, CultureInfo.InvariantCulture),
            world,
            null,
            null,
            null);
    }

    [Given("my inventory contains:")]
    public void GivenMyInventoryContains(Table table)
    {
        var world = Hooks.GetWorld(_context);

        foreach (var row in table.Rows)
        {
            world.Player.Carry(new ItemStack(
                row["Item"],
                row["Item"],
                row.ContainsKey("Quantity") ? row["Quantity"] : null,
                row.ContainsKey("Slot") ? row["Slot"] : null));
        }
    }

    private static double Number(string value)
    {
        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    private static Recipe NewRecipe(string name, Ingredient[] ingredients, string station)
    {
        return new Recipe(
            Id: name,
            ResultItemId: name,
            Name: name,
            Amount: 1,
            Station: station,
            Ingredients: ingredients);
    }
}
