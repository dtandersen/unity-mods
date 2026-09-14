namespace OdinsAi.Entity
{
    /// <summary>A creature near the player, with distance and compass direction.</summary>
    public sealed record Creature(string Name, float Distance, string Direction);
}