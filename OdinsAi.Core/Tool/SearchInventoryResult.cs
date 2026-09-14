using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A carried item as presented to the model.</summary>
    public sealed record ItemStackSummary(string Name, int Amount);

    /// <summary>Typed payload of the <c>search_inventory</c> tool.</summary>
    public sealed record InventorySearchResult(IReadOnlyList<ItemStackSummary> Items);
}