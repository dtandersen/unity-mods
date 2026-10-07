using System.Collections.Generic;

namespace StationeersAi.Repository;

/// <summary>In-memory housing lookup. Used by tests.</summary>
public sealed class InMemoryHousingRepository : IHousingRepository
{
    public readonly List<string?> Lookups = new List<string?>();

    /// <summary>The housing a lookup names, or null when no housing matches.</summary>
    public string? ResolveAs { get; set; } = "Climate Control";

    /// <summary>The housing lookups hand back.</summary>
    public InMemoryHousing Housing { get; } = new InMemoryHousing();

    public HousingLookup Find(string? device)
    {
        Lookups.Add(device);
        if (ResolveAs == null)
        {
            return new HousingLookup(null, $"Housing '{device}' not found.");
        }

        Housing.Name = ResolveAs;
        return new HousingLookup(Housing, null);
    }
}
