using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Entity;
using Reqnroll;

namespace OdinsAi.Test.Support
{
    /// <summary>Seeds the in-memory repositories (arrange for the suites).</summary>
    [Binding]
    public sealed class DatabaseSteps
    {
        private readonly ScenarioContext _context;

        public DatabaseSteps(ScenarioContext context)
        {
            _context = context;
        }

        [Given("the recipe database contains {string}")]
        public void GivenTheRecipeDatabaseContains(string name)
        {
            Hooks.GetWorld(_context).Recipes.Add(NewRecipe(name));
        }

        [Given("the recipe database contains a {string} recipe with ingredients:")]
        public void GivenTheRecipeDatabaseContainsARecipe(string name, Table table)
        {
            var ingredients = table.Rows
                .Select(row => new Ingredient(row["Ingredient"], int.Parse(row["Amount"])))
                .ToArray();

            Hooks.GetWorld(_context).Recipes.Add(NewRecipe(name, ingredients));
        }

        [Given("the item database contains food:")]
        public void GivenTheItemDatabaseContainsFood(Table table)
        {
            foreach (var row in table.Rows)
            {
                var stats = new FoodStats(
                    Number(row["Health"]),
                    Number(row["Stamina"]),
                    Number(row["Eitr"]),
                    Number(row["BurnTime"]),
                    Number(row["Regen"]));

                Hooks.GetWorld(_context).Items.Add(new Item(row["Name"], row["Name"], "food", stats));
            }
        }

        [Given("the item database contains:")]
        public void GivenTheItemDatabaseContains(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Items.Add(new Item(row["Name"], row["Name"], row["Type"], null));
            }
        }

        [Given("the nearby dungeons are:")]
        public void GivenTheNearbyDungeonsAre(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Dungeons.Add(new Dungeon(
                    row["Name"],
                    Number(row["Distance"]),
                    row["Direction"]));
            }
        }

        [Given("the nearby creatures are:")]
        public void GivenTheNearbyCreaturesAre(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Creatures.Add(new Creature(
                    row["Name"],
                    Number(row["Distance"]),
                    row["Direction"]));
            }
        }

        [Given("the nearby resources are:")]
        public void GivenTheNearbyResourcesAre(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Resources.Add(new ResourceNode(
                    row["Name"],
                    row["Type"],
                    Number(row["Distance"]),
                    row["Direction"]));
            }
        }

        [Given("the compendium contains:")]
        public void GivenTheCompendiumContains(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Compendium.Add(new CompendiumEntry(row["Topic"], row["Text"]));
            }
        }

        [Given("my position is {string}, {string}, {string} in the {string} biome")]
        public void GivenMyPositionIs(string x, string y, string z, string biome)
        {
            Hooks.GetWorld(_context).Location.Current = new Location(Number(x), Number(y), Number(z), biome);
        }

        [Given("the bosses are:")]
        public void GivenTheBossesAre(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Bosses.Add(new BossStatus(
                    row["Boss"],
                    row["Defeated"].Equals("Yes", System.StringComparison.OrdinalIgnoreCase)));
            }
        }

        [Given("my inventory contains:")]
        public void GivenMyInventoryContains(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var row in table.Rows)
            {
                world.Inventory.Add(new ItemStack(
                    row["Item"], row["Item"], int.Parse(row["Amount"]), 1, row["Type"]));
            }
        }

        [Given("the nearby containers contain:")]
        public void GivenTheNearbyContainersContain(Table table)
        {
            var world = Hooks.GetWorld(_context);

            foreach (var group in table.Rows.GroupBy(row => row["Container"]))
            {
                var items = group
                    .Select(row => new ItemStack(
                        row["Item"], row["Item"], int.Parse(row["Amount"]), 1, row["Type"]))
                    .ToArray();

                world.Storage.Add(new StorageContainer(group.Key, items));
            }
        }

        private static float Number(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static Recipe NewRecipe(string name, IReadOnlyList<Ingredient> ingredients = null)
        {
            return new Recipe(
                Id: name,
                ResultItemId: name,
                Name: name,
                Amount: 1,
                CraftingStation: "cauldron",
                Ingredients: ingredients ?? Array.Empty<Ingredient>());
        }
    }
}