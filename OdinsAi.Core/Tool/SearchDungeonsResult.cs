using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A dungeon entrance as presented to the model.</summary>
    public sealed record DungeonSummary(string Name, float Distance, string Direction);

    /// <summary>Typed payload of the <c>search_dungeons</c> tool.</summary>
    public sealed record SearchDungeonsResult(IReadOnlyList<DungeonSummary> Dungeons);
}