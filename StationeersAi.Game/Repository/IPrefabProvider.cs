using System.Collections.Generic;
using Assets.Scripts.Objects;

namespace StationeersAi.Repository;

/// <summary>
/// Supplies the current prefab catalogue on every call, so readers hold
/// no snapshot and never go stale.
/// </summary>
public interface IPrefabProvider
{
    IEnumerable<Thing>? GetPrefabs();
}
