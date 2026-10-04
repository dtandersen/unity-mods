using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory IC export. Used by tests.</summary>
public sealed class InMemoryIcExport : IIcExport
{
    public readonly List<(string? Computer, string? Device)> Exports = new List<(string? Computer, string? Device)>();

    /// <summary>The result exports return.</summary>
    public IcWriteResult Result { get; set; } = new IcWriteResult(true, "Climate Control", 37, null);

    public IcWriteResult Export(string? computer, string? device)
    {
        Exports.Add((computer, device));
        return Result;
    }
}
