using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over recipes. Kept out of the repository so both the in-memory and
    /// Valheim-backed repositories share identical, testable match semantics.
    /// Matching is case-insensitive substring on the recipe name; order is not
    /// significant.
    /// </summary>
    public sealed class RecipeIndex
    {
        private readonly IRecipeRepository _recipes;

        public RecipeIndex(IRecipeRepository recipes)
        {
            _recipes = recipes;
        }

        public IReadOnlyList<Recipe> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Recipe>();
            }

            return _recipes.GetAll()
                .Where(recipe => recipe.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
        }
    }
}