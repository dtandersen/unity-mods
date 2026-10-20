using System.Collections.Generic;
using System.Linq;
using OdinsAi.Query;
using OdinsAi.Repository;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find recipes by name and report unlock state.</summary>
    public sealed class SearchRecipesCommand
    {
        private readonly RecipeIndex _recipes;
        private readonly IPlayerContext _player;

        public SearchRecipesCommand(RecipeIndex recipes, IPlayerContext player)
        {
            _recipes = recipes;
            _player = player;
        }

        public IReadOnlyList<UnlockedRecipe> Execute(string query)
        {
            return _recipes.Search(query)
                .Select(recipe => new UnlockedRecipe(recipe, _player.IsRecipeKnown(recipe.Id)))
                .ToArray();
        }
    }
}