using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A food item's stats as presented to the model.</summary>
    public sealed record FoodSummary(
        string Name,
        float Health,
        float Stamina,
        float Eitr,
        float BurnTime,
        float Regen);

    /// <summary>Typed payload of the <c>search_food</c> tool.</summary>
    public sealed record SearchFoodResult(IReadOnlyList<FoodSummary> Foods);
}