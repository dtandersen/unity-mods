using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory IC devices by thing id. Used by tests.</summary>
public sealed class InMemoryIcDevices : IIcDevices
{
    public readonly List<long> ComputerLookups = new List<long>();

    public readonly List<long> HousingLookups = new List<long>();

    /// <summary>Set false so a computer lookup refuses the id.</summary>
    public bool ComputerFound { get; set; } = true;

    /// <summary>Set false so a housing lookup refuses the id.</summary>
    public bool HousingFound { get; set; } = true;

    /// <summary>The computer lookups hand back.</summary>
    public InMemoryComputer Computer { get; } = new InMemoryComputer();

    /// <summary>The housing lookups hand back: the one computer's network holds.</summary>
    public InMemoryHousing Housing { get; } = new InMemoryHousing();

    public InMemoryIcDevices()
    {
        Computer.NetworkedHousing = Housing;
    }

    public IComputer FindComputer(long computerId)
    {
        ComputerLookups.Add(computerId);
        if (!ComputerFound)
        {
            throw new ThingNotFound("Computer", computerId);
        }

        return Computer;
    }

    public IHousing FindHousing(long thingId)
    {
        HousingLookups.Add(thingId);
        if (!HousingFound)
        {
            throw new ThingNotFound("Housing", thingId);
        }

        return Housing;
    }
}
