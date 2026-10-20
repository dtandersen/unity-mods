using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>Typed payload of the <c>search_storage</c> tool (totals per item).</summary>
    public sealed record StorageSearchResult(IReadOnlyList<ItemStackSummary> Items);
}