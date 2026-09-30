namespace StationeersAi.Entity
{
    /// <summary>
    /// A stack of items carried by the player. <see cref="Quantity"/> is the
    /// game's own quantity text (e.g. <c>x5</c>, <c>75%</c>), or null when the
    /// item has no meaningful quantity.
    /// </summary>
    public sealed record ItemStack(string Id, string Name, string Quantity, string Slot);
}
