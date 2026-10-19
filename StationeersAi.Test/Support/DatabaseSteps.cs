using System;
using System.Collections.Generic;
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
            world.Items.Add(new Item(row["Name"], row["Name"], row["ItemType"]));
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
                row.ContainsKey("Description") ? row["Description"] : null,
                Array.Empty<BuildStep>()));
        }
    }

    [Given("the structure {string} has build states:")]
    public void GivenTheStructureHasBuildStates(string name, Table table)
    {
        var world = Hooks.GetWorld(_context);
        var current = world.Structures.GetById(name);
        world.Structures.Update(new Structure(
            name,
            current?.Name ?? name,
            current?.Description,
            Steps(table)));
    }

    internal static IReadOnlyList<BuildStep> Steps(Table table)
    {
        return table.Rows
            .GroupBy(row => int.Parse(row["State"], CultureInfo.InvariantCulture))
            .Select(group => new BuildStep(
                group.Key,
                group.Select(row => new BuildEntry(
                    row["PrefabName"],
                    row.ContainsKey("Name") && !string.IsNullOrWhiteSpace(row["Name"]) ? row["Name"].Trim() : row["PrefabName"],
                    row.ContainsKey("Quantity") && !string.IsNullOrWhiteSpace(row["Quantity"])
                        ? double.Parse(row["Quantity"], CultureInfo.InvariantCulture)
                        : (double?)null)).ToArray()))
            .ToArray();
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
                row.ContainsKey("Text") ? row["Text"] : null,
                null,
                null));
        }
    }

    [Given("the stationpedia page {string} is made in:")]
    public void GivenTheStationpediaPageIsMadeIn(string key, Table table)
    {
        var world = Hooks.GetWorld(_context);
        var current = world.Stationpedia.GetByKey(key);
        world.Stationpedia.Update(new StationpediaEntry(
            key,
            current?.Title ?? key,
            current?.Description,
            current?.Text,
            current?.UsedIn,
            table.Rows.Select(row => new StationpediaMake(
                row["Station"],
                row.ContainsKey("Tier") && !string.IsNullOrWhiteSpace(row["Tier"]) ? row["Tier"] : null,
                row.ContainsKey("Detail") && !string.IsNullOrWhiteSpace(row["Detail"]) ? row["Detail"] : null,
                row.ContainsKey("Key") && !string.IsNullOrWhiteSpace(row["Key"]) ? row["Key"] : null)).ToArray()));
    }
    [Given("the stationpedia page {string} is used in:")]
    public void GivenTheStationpediaPageIsUsedIn(string key, Table table)
    {
        var world = Hooks.GetWorld(_context);
        var current = world.Stationpedia.GetByKey(key);
        world.Stationpedia.Update(new StationpediaEntry(
            key,
            current?.Title ?? key,
            current?.Description,
            current?.Text,
            table.Rows.Select(row => new StationpediaUse(
                row["Title"],
                row.ContainsKey("Detail") && !string.IsNullOrWhiteSpace(row["Detail"]) ? row["Detail"] : null,
                row.ContainsKey("Key") && !string.IsNullOrWhiteSpace(row["Key"]) ? row["Key"] : null)).ToArray(),
            current?.HowToBuild));
    }

    [Given("my position is {string}, {string}, {string} on {string}")]
    public void GivenMyPositionIs(string x, string y, string z, string world)
    {
        Hooks.GetWorld(_context).Player.Status = new Player(
            float.Parse(x, CultureInfo.InvariantCulture),
            float.Parse(y, CultureInfo.InvariantCulture),
            float.Parse(z, CultureInfo.InvariantCulture),
            world,
            null,
            null,
            null);
    }

    [Given("my position is unknown")]
    public void GivenMyPositionIsUnknown()
    {
        Hooks.GetWorld(_context).Player.Status = null;
    }

    [Given("my inventory contains:")]
    public void GivenMyInventoryContains(Table table)
    {
        var world = Hooks.GetWorld(_context);

        foreach (var row in table.Rows)
        {
            var slot = row.ContainsKey("Slot") ? row["Slot"] : null;
            world.Player.Carry(new ItemStack(
                row["Item"],
                row["Item"],
                row.ContainsKey("Quantity") ? row["Quantity"] : null,
                slot,
                row.ContainsKey("Hand") ? row["Hand"] : HandOf(slot)));
        }
    }

    private static string HandOf(string slot)
    {
        if (string.Equals(slot, "Left hand", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Left";
        }

        if (string.Equals(slot, "Right hand", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Right";
        }

        return null;
    }

    internal static IReadOnlyList<Ingredient> Materials(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<Ingredient>();
        }

        // One material per ";", each as "<name> x<amount>", e.g. "Iron Sheets x4".
        return value.Split(';')
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .Select(part =>
            {
                var mark = part.LastIndexOf('x');
                if (mark > 0 && double.TryParse(part.Substring(mark + 1).Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var amount))
                {
                    return new Ingredient(part.Substring(0, mark).Trim(), amount);
                }

                return new Ingredient(part, 1);
            })
            .ToArray();
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
