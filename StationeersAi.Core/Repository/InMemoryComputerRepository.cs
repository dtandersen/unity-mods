using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory computer lookup by thing id. Used by tests.</summary>
public sealed class InMemoryComputerRepository : IComputerRepository
{
    public readonly List<long> Lookups = new List<long>();

    /// <summary>Set false so lookups report no computer found.</summary>
    public bool Found { get; set; } = true;

    /// <summary>The computer lookups hand back.</summary>
    public InMemoryComputer Computer { get; } = new InMemoryComputer();

    public ComputerLookup Find(long computerId)
    {
        Lookups.Add(computerId);
        if (!Found)
        {
            return new ComputerLookup(null, ThingNotFound.Describe("Computer", computerId));
        }

        return new ComputerLookup(Computer, null);
    }
}
