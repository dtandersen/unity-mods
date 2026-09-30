using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Recipes matching a search.</summary>
    public sealed record SearchRecipesResult(IReadOnlyList<Recipe> Recipes);
}
