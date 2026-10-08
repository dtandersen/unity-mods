using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Ask for one Stationpedia page by key.</summary>
public sealed record ReadStationpediaRequest(string? Key);

/// <summary>The page, or why none answers to the key.</summary>
public sealed record PageResult(Status Status, List<string> Errors, StationpediaEntry? Page) : BaseResult(Status, Errors);

/// <summary>Domain operation: read one Stationpedia page by key.</summary>
public sealed class ReadStationpediaCommand : Command<ReadStationpediaRequest, PageResult>
{
    private readonly IStationpediaRepository _pages;

    public ReadStationpediaCommand(IStationpediaRepository pages)
    {
        _pages = pages;
    }

    public PageResult Execute(ReadStationpediaRequest request)
    {
        var entry = string.IsNullOrWhiteSpace(request.Key) ? null : _pages.GetByKey(request.Key);
        return entry == null
            ? new PageResult(
                Status.NOT_FOUND,
                new List<string> { $"No Stationpedia page found for key '{request.Key}'." },
                null)
            : new PageResult(Status.SUCCESS, new List<string>(), entry);
    }
}
