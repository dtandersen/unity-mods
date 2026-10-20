using System.Collections.Generic;

namespace OdinsAi.Entity
{
    /// <summary>A container (chest, cart, ...) and the item stacks inside it.</summary>
    public sealed record StorageContainer(string Name, IReadOnlyList<ItemStack> Items);
}