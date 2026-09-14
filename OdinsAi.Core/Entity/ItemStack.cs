namespace OdinsAi.Entity
{
    /// <summary>
    /// A stack of items carried by the player or stored in a container.
    /// <see cref="Type"/> is a coarse category such as <c>food</c>, <c>material</c>
    /// or <c>tool</c>.
    /// </summary>
    public sealed record ItemStack(string Id, string Name, int Amount, int Quality, string Type);
}