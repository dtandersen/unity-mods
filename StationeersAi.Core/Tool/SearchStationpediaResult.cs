using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Stationpedia pages matching a search.</summary>
    public sealed record SearchStationpediaResult(IReadOnlyList<StationpediaEntry> Entries);
}
