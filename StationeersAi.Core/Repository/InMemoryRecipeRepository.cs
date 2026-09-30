using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory recipe repository. Used by tests.</summary>
    public sealed class InMemoryRecipeRepository : IRecipeRepository
    {
        private readonly List<Recipe> _recipes;

        public InMemoryRecipeRepository(IEnumerable<Recipe> recipes = null)
        {
            _recipes = recipes?.ToList() ?? new List<Recipe>();
        }

        public void Add(Recipe recipe)
        {
            _recipes.Add(recipe);
        }

        public IReadOnlyList<Recipe> GetAll()
        {
            return _recipes;
        }

        public Recipe GetById(string id)
        {
            return _recipes.FirstOrDefault(recipe => recipe.Id == id);
        }
    }
}
