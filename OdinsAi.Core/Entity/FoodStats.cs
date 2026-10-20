namespace OdinsAi.Entity
{
    /// <summary>Nutritional stats of a food item.</summary>
    /// <param name="Health">Health restored.</param>
    /// <param name="Stamina">Stamina restored.</param>
    /// <param name="Eitr">Eitr restored.</param>
    /// <param name="BurnTime">Seconds the food stays active.</param>
    /// <param name="Regen">Health regen per tick while active.</param>
    public sealed record FoodStats(
        float Health,
        float Stamina,
        float Eitr,
        float BurnTime,
        float Regen);
}