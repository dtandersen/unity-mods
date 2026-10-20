using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>An item's name and type as presented to the model.</summary>
    public sealed record ItemSummary(string Name, string Type);

    /// <summary>Typed payload of the <c>search_items</c> tool.</summary>
    public sealed record SearchItemsResult(IReadOnlyList<ItemSummary> Items);
}