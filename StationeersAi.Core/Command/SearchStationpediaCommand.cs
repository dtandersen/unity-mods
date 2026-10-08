using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Ask for help pages by name fragment.</summary>
public sealed record SearchStationpediaRequest(string? Query);

/// <summary>The pages that matched; empty when none did.</summary>
public sealed record StationpediaSearchResult(
    Status Status, List<string> Errors, IReadOnlyList<StationpediaEntry> Pages) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the Stationpedia catalogue.</summary>
public sealed class SearchStationpediaCommand : Command<SearchStationpediaRequest, StationpediaSearchResult>
{
    private readonly StationpediaIndex _pages;

    public SearchStationpediaCommand(StationpediaIndex pages)
    {
        _pages = pages;
    }

    public StationpediaSearchResult Execute(SearchStationpediaRequest request)
    {
        return new StationpediaSearchResult(Status.SUCCESS, new List<string>(), _pages.Search(request.Query));
    }
}
