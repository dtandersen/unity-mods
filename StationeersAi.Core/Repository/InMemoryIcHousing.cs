using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory IC housing reader. Used by tests.</summary>
public sealed class InMemoryIcHousing : IIcHousing
{
    public readonly List<string?> Reads = new List<string?>();

    /// <summary>The housing a read resolves to, or null when none matches.</summary>
    public string? ResolveAs { get; set; } = "Climate Control";

    /// <summary>The text reads return as the chip contents.</summary>
    public string HousingCode { get; set; } = string.Empty;

    public IcReadResult Read(string? device)
    {
        Reads.Add(device);
        if (ResolveAs == null)
        {
            return new IcReadResult(false, null, null, $"Housing '{device}' not found.");
        }

        return new IcReadResult(true, ResolveAs, HousingCode, null);
    }
}
