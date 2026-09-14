using System;
using System.Linq;
using OdinsAi.Entity;
using Reqnroll;

namespace OdinsAi.Test.Support
{
    /// <summary>Steps that seed the recipe repository (arrange for both suites).</summary>
    [Binding]
    public sealed class RecipeDatabaseSteps
    {
        private readonly ScenarioContext _context;

        public RecipeDatabaseSteps(ScenarioContext context)
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

        private static Recipe NewRecipe(string name, System.Collections.Generic.IReadOnlyList<Ingredient> ingredients = null)
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