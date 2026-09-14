namespace OdinsAi.Entity
{
    /// <summary>
    /// An item from the game. <see cref="Food"/> is null for anything that is not
    /// edible.
    /// </summary>
    public sealed record Item(string Id, string Name, FoodStats Food);
}