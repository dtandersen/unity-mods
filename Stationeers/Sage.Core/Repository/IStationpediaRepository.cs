using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>Read-only access to the Stationpedia pages.</summary>
public interface IStationpediaRepository
{
    /// <summary>All known pages. Order is not significant.</summary>
    IReadOnlyList<StationpediaEntry> GetAll();

    /// <summary>The page with the given key, or null if none exists.</summary>
    StationpediaEntry? GetByKey(string? key);
}
