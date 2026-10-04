using System.Collections.Generic;
using Assets.Scripts.Objects.Items;

namespace StationeersAi.Repository;

/// <summary>
/// Supplies the recipe table entries on every call, so readers hold no
/// snapshot and never go stale.
/// </summary>
public interface IRecipeProvider
{
    IEnumerable<RecipeReference> GetRecipes();
}
