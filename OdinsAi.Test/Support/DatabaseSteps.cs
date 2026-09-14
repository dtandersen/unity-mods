using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Entity;
using Reqnroll;

namespace OdinsAi.Test.Support
{
    /// <summary>Seeds the in-memory repositories (arrange for both suites).</summary>
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

                Hooks.GetWorld(_context).Items.Add(new Item(row["Name"], row["Name"], stats));
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