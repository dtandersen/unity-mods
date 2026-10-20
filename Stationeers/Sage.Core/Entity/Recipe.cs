using System.Collections.Generic;

namespace Sage.Entity;

/// <summary>A manufacturing recipe from the game.</summary>
/// <param name="Id">Stable identity of the recipe (the result prefab name).</param>
/// <param name="ResultItemId">Prefab name of the item the recipe produces.</param>
/// <param name="Name">Display name of the result, used for searching.</param>
/// <param name="Amount">How many of the result item is produced.</param>
/// <param name="Station">Machine that runs the recipe (e.g. Autolathe), or null.</param>
/// <param name="Ingredients">The materials the recipe consumes.</param>
public sealed record Recipe(
    string Id,
    string ResultItemId,
    string Name,
    int Amount,
    string? Station,
    IReadOnlyList<Ingredient> Ingredients);
