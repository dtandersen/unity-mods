using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: find recipes by name.</summary>
    public sealed class SearchRecipesCommand
    {
        private readonly RecipeIndex _recipes;

        public SearchRecipesCommand(RecipeIndex recipes)
        {
            _recipes = recipes;
        }

        public IReadOnlyList<Recipe> Execute(string query)
        {
            return _recipes.Search(query);
        }
    }
}
