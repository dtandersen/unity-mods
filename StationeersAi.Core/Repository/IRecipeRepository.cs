using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>Read-only access to the recipe "database".</summary>
    public interface IRecipeRepository
    {
        /// <summary>All known recipes. Order is not significant.</summary>
        IReadOnlyList<Recipe> GetAll();

        /// <summary>The recipe with the given id, or null if none exists.</summary>
        Recipe GetById(string id);
    }
}
