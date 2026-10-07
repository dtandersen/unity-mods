using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory housing lookup by thing id. Used by tests.</summary>
public sealed class InMemoryHousingRepository : IHousingRepository
{
    public readonly List<long> Lookups = new List<long>();

    /// <summary>Set false so lookups refuse the id.</summary>
    public bool Found { get; set; } = true;

    /// <summary>The housing lookups hand back.</summary>
    public InMemoryHousing Housing { get; } = new InMemoryHousing();

    public IHousing Find(long thingId)
    {
        Lookups.Add(thingId);
        if (!Found)
        {
            throw new ThingNotFound("Housing", thingId);
        }

        return Housing;
    }
}
