using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory housing lookup by thing id. Used by tests.</summary>
public sealed class InMemoryHousingRepository : IHousingRepository
{
    public readonly List<long> Lookups = new List<long>();

    /// <summary>Set false so lookups report no housing found.</summary>
    public bool Found { get; set; } = true;

    /// <summary>The housing lookups hand back.</summary>
    public InMemoryHousing Housing { get; } = new InMemoryHousing();

    public HousingLookup Find(long thingId)
    {
        Lookups.Add(thingId);
        if (!Found)
        {
            return new HousingLookup(null, ThingNotFound.Describe("Housing", thingId));
        }

        return new HousingLookup(Housing, null);
    }
}
