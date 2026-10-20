using System.Collections.Generic;
using Assets.Scripts.Objects.Items;

namespace Sage.Repository;

/// <summary>Reads the recipe tables straight from the game.</summary>
public sealed class StationeersRecipeProvider : IRecipeProvider
{
    public IEnumerable<RecipeReference> GetRecipes()
    {
        return StationeersRecipeRepository.RecipeTables();
    }
}
