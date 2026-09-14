using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A boss and its defeat status as presented to the model.</summary>
    public sealed record BossSummary(string Name, bool Defeated);

    /// <summary>Typed payload of the <c>list_bosses</c> tool.</summary>
    public sealed record ListBossesResult(IReadOnlyList<BossSummary> Bosses);
}