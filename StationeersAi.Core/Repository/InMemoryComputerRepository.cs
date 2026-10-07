using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory computer lookup. Used by tests.</summary>
public sealed class InMemoryComputerRepository : IComputerRepository
{
    public readonly List<string?> Lookups = new List<string?>();

    /// <summary>The computer a lookup names, or null when no computer matches.</summary>
    public string? ResolveAs { get; set; } = "Lab Computer";

    /// <summary>The computer lookups hand back.</summary>
    public InMemoryComputer Computer { get; } = new InMemoryComputer();

    public ComputerLookup Find(string? computer)
    {
        Lookups.Add(computer);
        if (ResolveAs == null)
        {
            return new ComputerLookup(null, $"Computer '{computer}' not found.");
        }

        Computer.Name = ResolveAs;
        return new ComputerLookup(Computer, null);
    }
}
