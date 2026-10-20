namespace OdinsAi.Entity
{
    /// <summary>
    /// A resource node near the player (ore deposit, pickable, tree), with distance
    /// and compass direction. <see cref="Type"/> is <c>ore</c>, <c>pickable</c> or <c>tree</c>.
    /// </summary>
    public sealed record ResourceNode(string Name, string Type, float Distance, string Direction);
}