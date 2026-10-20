using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A resource node as presented to the model.</summary>
    public sealed record ResourceSummary(string Name, string Type, float Distance, string Direction);

    /// <summary>Typed payload of the <c>search_resources</c> tool.</summary>
    public sealed record SearchResourcesResult(IReadOnlyList<ResourceSummary> Resources);
}