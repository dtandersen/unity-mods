namespace OdinsAi.Entity
{
    /// <summary>An ingredient required by a recipe.</summary>
    public sealed record Ingredient(string ItemId, int Amount);
}