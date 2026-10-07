using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory computer lookup by thing id. Used by tests.</summary>
public sealed class InMemoryComputerRepository : IComputerRepository
{
    public readonly List<long> Lookups = new List<long>();

    /// <summary>Set false so lookups refuse the id.</summary>
    public bool Found { get; set; } = true;

    /// <summary>The computer lookups hand back.</summary>
    public InMemoryComputer Computer { get; } = new InMemoryComputer();

    public IComputer Find(long computerId)
    {
        Lookups.Add(computerId);
        if (!Found)
        {
            throw new ThingNotFound("Computer", computerId);
        }

        return Computer;
    }
}
