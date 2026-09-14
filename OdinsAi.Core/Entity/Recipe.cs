using System.Collections.Generic;

namespace OdinsAi.Entity
{
    /// <summary>A crafting recipe from the game.</summary>
    /// <param name="Id">Stable identity of the recipe (the result item id).</param>
    /// <param name="ResultItemId">Item the recipe produces.</param>
    /// <param name="Name">Display name of the result, used for searching.</param>
    /// <param name="Amount">How many of the result item is produced.</param>
    /// <param name="CraftingStation">Required station, or null for hand-crafting.</param>
    /// <param name="Ingredients">The materials the recipe consumes.</param>
    public sealed record Recipe(
        string Id,
        string ResultItemId,
        string Name,
        int Amount,
        string CraftingStation,
        IReadOnlyList<Ingredient> Ingredients);
}