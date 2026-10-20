using System.Collections.Generic;
using Assets.Scripts.UI;

namespace Sage.Repository;

/// <summary>
/// Supplies the encyclopedia pages on every call, so readers hold no
/// snapshot and never go stale.
/// </summary>
public interface IStationpediaProvider
{
    IEnumerable<StationpediaPage>? GetPages();
}
