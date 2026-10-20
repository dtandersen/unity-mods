using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>An ingredient as presented to the model.</summary>
    public sealed record IngredientResult(string ItemId, int Amount);

    /// <summary>A recipe as presented to the model.</summary>
    public sealed record RecipeSummary(
        string Name,
        string CraftingStation,
        IReadOnlyList<IngredientResult> Ingredients,
        bool Unlocked);

    /// <summary>Typed payload of the <c>search_recipes</c> tool.</summary>
    public sealed record SearchRecipesResult(IReadOnlyList<RecipeSummary> Recipes);
}