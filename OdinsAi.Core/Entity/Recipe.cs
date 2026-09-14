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
    /// <param name="Upgrades">
    /// Cost of each quality upgrade, or null when none are known. Quality 1 (the
    /// base craft) is not included; see <see cref="Ingredients"/>.
    /// </param>
    /// <param name="Unlocked">Whether the player has discovered this recipe.</param>
    public sealed record Recipe(
        string Id,
        string ResultItemId,
        string Name,
        int Amount,
        string CraftingStation,
        IReadOnlyList<Ingredient> Ingredients,
        IReadOnlyList<UpgradeLevel> Upgrades = null,
        bool Unlocked = false);
}