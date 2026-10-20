namespace OdinsAi.Entity
{
    /// <summary>
    /// An item from the game. <see cref="Food"/> is null for anything that is not
    /// edible. <see cref="Type"/> is a coarse category such as <c>food</c>,
    /// <c>material</c> or <c>twohandedweapon</c>.
    /// </summary>
    public sealed record Item(string Id, string Name, string Type, FoodStats Food);
}