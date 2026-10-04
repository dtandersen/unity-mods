using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Domain operation: find Stationpedia pages by title.</summary>
public sealed class SearchStationpediaCommand
{
    private readonly StationpediaIndex _pages;

    public SearchStationpediaCommand(StationpediaIndex pages)
    {
        _pages = pages;
    }

    public IReadOnlyList<StationpediaEntry> Execute(string? query)
    {
        return _pages.Search(query);
    }
}
