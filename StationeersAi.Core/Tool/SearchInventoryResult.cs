using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Carried items matching a search.</summary>
    public sealed record SearchInventoryResult(IReadOnlyList<ItemStack> Stacks);
}
