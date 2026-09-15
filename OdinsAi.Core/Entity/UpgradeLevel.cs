using System.Collections.Generic;

namespace OdinsAi.Entity
{
    /// <summary>The materials needed to raise an item to <paramref name="Quality"/>.</summary>
    /// <param name="Quality">Target quality (2 and up; quality 1 is the base craft).</param>
    /// <param name="Station">Required station for this quality, or null if none.</param>
    /// <param name="StationLevel">Minimum station level needed at this quality.</param>
    /// <param name="Ingredients">The materials consumed by this upgrade step.</param>
    public sealed record UpgradeLevel(
        int Quality,
        string Station,
        int StationLevel,
        IReadOnlyList<Ingredient> Ingredients);
}