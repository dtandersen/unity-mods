using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>Carried items matching a search.</summary>
public sealed record SearchInventoryResult(IReadOnlyList<ItemStack> Stacks);
