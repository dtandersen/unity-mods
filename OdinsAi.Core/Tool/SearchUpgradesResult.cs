using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>An upgrade step as presented to the model.</summary>
    public sealed record UpgradeSummary(
        string Item,
        int Quality,
        string Station,
        int StationLevel,
        IReadOnlyList<IngredientResult> Ingredients,
        bool Unlocked);

    /// <summary>Typed payload of the <c>search_upgrades</c> tool.</summary>
    public sealed record SearchUpgradesResult(IReadOnlyList<UpgradeSummary> Upgrades);
}