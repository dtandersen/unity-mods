using System.Collections.Generic;
using Assets.Scripts.UI;

namespace Sage.Repository;

/// <summary>Reads the encyclopedia pages straight from the game.</summary>
public sealed class StationeersStationpediaProvider : IStationpediaProvider
{
    public IEnumerable<StationpediaPage>? GetPages()
    {
        return Stationpedia.StationpediaPages;
    }
}
