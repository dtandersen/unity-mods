using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read one Stationpedia page by key.</summary>
public sealed class ReadStationpediaCommand
{
    private readonly IStationpediaRepository _pages;

    public ReadStationpediaCommand(IStationpediaRepository pages)
    {
        _pages = pages;
    }

    public StationpediaEntry? Execute(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return _pages.GetByKey(key);
    }
}
