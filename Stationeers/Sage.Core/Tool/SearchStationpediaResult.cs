using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>Stationpedia pages matching a search.</summary>
public sealed record SearchStationpediaResult(IReadOnlyList<StationpediaEntry> Entries);
