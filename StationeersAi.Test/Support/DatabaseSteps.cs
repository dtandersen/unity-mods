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
                null,
                null,
                null,
                NumberOrNull(row, "PowerGenerationW"),
                NumberOrNull(row, "PowerStorageJ"),
                NumberOrNull(row, "PowerDrawW"),
                NumberOrNull(row, "FuelEnergyPerSecondW"),
                NumberOrNull(row, "TickSeconds")));
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
                row.ContainsKey("Key") && !string.IsNullOrWhiteSpace(row["Key"]) ? row["Key"] : null)).ToArray(),
            current?.Combustion,
            current?.BuildSteps,
            current?.PowerGenerationW,
            current?.PowerStorageJ,
            current?.PowerDrawW,
            current?.FuelEnergyPerSecondW,
            current?.TickSeconds));
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
                row.ContainsKey("Key") && !string.IsNullOrWhiteSpace(row["Key"]) ? row["Key"] : null,
                NumberOrNull(row, "EnergyPerSecondW"),
                NumberOrNull(row, "GeneratorPowerW"),
                IntOrNull(row, "BurnTicks"),
                NumberOrNull(row, "TotalJoules"),
                NumberOrNull(row, "BurnSeconds"),
                NumberOrNull(row, "TickSeconds"))).ToArray(),
            current?.HowToBuild,
            current?.Combustion,
            current?.BuildSteps,
            current?.PowerGenerationW,
            current?.PowerStorageJ,
            current?.PowerDrawW,
            current?.FuelEnergyPerSecondW,
            current?.TickSeconds));
    }

    private static double? NumberOrNull(dynamic row, string column)
    {
        if (!row.ContainsKey(column) || string.IsNullOrWhiteSpace(row[column]))
        {
            return null;
        }

        return double.Parse(row[column], CultureInfo.InvariantCulture);
    }

    private static int? IntOrNull(dynamic row, string column)
    {
        if (!row.ContainsKey(column) || string.IsNullOrWhiteSpace(row[column]))
        {
            return null;
        }

        return int.Parse(row[column], CultureInfo.InvariantCulture);
    }

    [Given("the stationpedia page {string} burns:")]
    public void GivenTheStationpediaPageBurns(string key, Table table)
    {
        var world = Hooks.GetWorld(_context);
        var current = world.Stationpedia.GetByKey(key);
        world.Stationpedia.Update(new StationpediaEntry(
            key,
            current?.Title ?? key,
            current?.Description,
            current?.Text,
            current?.UsedIn,
            current?.HowToBuild,
            table.Rows.Select(row => new StationpediaCombustion(
                row["Fuel"],
                row["Oxidiser"],
                row.ContainsKey("FuelAmount") && !string.IsNullOrWhiteSpace(row["FuelAmount"])
                    ? double.Parse(row["FuelAmount"], CultureInfo.InvariantCulture)
                    : 1,
                row.ContainsKey("OxidiserAmount") && !string.IsNullOrWhiteSpace(row["OxidiserAmount"])
                    ? double.Parse(row["OxidiserAmount"], CultureInfo.InvariantCulture)
                    : 1,
                Outputs(row.ContainsKey("Outputs") ? row["Outputs"] : null),
                row.ContainsKey("AutoIgnitionC") && !string.IsNullOrWhiteSpace(row["AutoIgnitionC"])
                    ? double.Parse(row["AutoIgnitionC"], CultureInfo.InvariantCulture)
                    : (double?)null,
                row.ContainsKey("OutputTemperatureC") && !string.IsNullOrWhiteSpace(row["OutputTemperatureC"])
                    ? double.Parse(row["OutputTemperatureC"], CultureInfo.InvariantCulture)
                    : (double?)null)).ToArray(),
            current?.BuildSteps,
            current?.PowerGenerationW,
            current?.PowerStorageJ,
            current?.PowerDrawW,
            current?.FuelEnergyPerSecondW,
            current?.TickSeconds));
    }

    internal static IReadOnlyList<StationpediaCombustionOutput> Outputs(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<StationpediaCombustionOutput>();
        }

        // One output per ";", each as "<gas> x<amount>", e.g. "CO2 x6; X x3".
        return value.Split(';')
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .Select(part =>
            {
                var mark = part.LastIndexOf('x');
                if (mark > 0 && double.TryParse(part.Substring(mark + 1).Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var amount))
                {
                    return new StationpediaCombustionOutput(part.Substring(0, mark).Trim(), amount);
                }

                return new StationpediaCombustionOutput(part, 1);
            })
            .ToArray();
    }

    [Given("the stationpedia page {string} builds in:")]
    public void GivenTheStationpediaPageBuildsIn(string key, Table table)
    {
        var world = Hooks.GetWorld(_context);
        var current = world.Stationpedia.GetByKey(key);
        world.Stationpedia.Update(new StationpediaEntry(
            key,
            current?.Title ?? key,
            current?.Description,
            current?.Text,
            current?.UsedIn,
            current?.HowToBuild,
            current?.Combustion,
            table.Rows.Select(row => row["Step"]).ToArray(),
            current?.PowerGenerationW,
            current?.PowerStorageJ,
            current?.PowerDrawW,
            current?.FuelEnergyPerSecondW,
            current?.TickSeconds));
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
