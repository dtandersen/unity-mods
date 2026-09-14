using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find recipes (and their upgrade costs) by name.</summary>
    public sealed class SearchUpgradesCommand
    {
        private readonly RecipeIndex _recipes;

        public SearchUpgradesCommand(RecipeIndex recipes)
        {
            _recipes = recipes;
        }

        public IReadOnlyList<Recipe> Execute(string query)
        {
            return _recipes.Search(query);
        }
    }
}