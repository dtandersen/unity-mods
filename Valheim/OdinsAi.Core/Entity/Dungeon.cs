namespace OdinsAi.Entity
{
    /// <summary>
    /// A dungeon entrance in the world, with its distance from the player and the
    /// 8-way compass direction to it (N, NE, E, ...).
    /// </summary>
    public sealed record Dungeon(string Name, float Distance, string Direction);
}