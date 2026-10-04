using System.Collections.Generic;
using Assets.Scripts.Objects;

namespace StationeersAi.Repository;

/// <summary>Reads the live prefab catalogue straight from the game.</summary>
public sealed class StationeersPrefabProvider : IPrefabProvider
{
    public IEnumerable<Thing>? GetPrefabs()
    {
        return Prefab.AllPrefabs;
    }
}
