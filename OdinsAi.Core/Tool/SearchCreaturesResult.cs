using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A creature as presented to the model.</summary>
    public sealed record CreatureSummary(string Name, float Distance, string Direction);

    /// <summary>Typed payload of the <c>search_creatures</c> tool.</summary>
    public sealed record SearchCreaturesResult(IReadOnlyList<CreatureSummary> Creatures);
}