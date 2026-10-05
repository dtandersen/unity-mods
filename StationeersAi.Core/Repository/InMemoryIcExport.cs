using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory IC export. Used by tests.</summary>
public sealed class InMemoryIcExport : IIcExport
{
    public readonly List<(string? Computer, string? Device)> Exports = new List<(string? Computer, string? Device)>();

    public readonly List<(string? Computer, string? Device)> Imports = new List<(string? Computer, string? Device)>();

    /// <summary>The result exports return.</summary>
    public IcWriteResult Result { get; set; } = new IcWriteResult(true, "Climate Control", 37, null);

    /// <summary>The result imports return.</summary>
    public IcWriteResult ImportResult { get; set; } = new IcWriteResult(true, "Lab Computer", 3, null);

    public IcWriteResult Export(string? computer, string? device)
    {
        Exports.Add((computer, device));
        return Result;
    }

    public IcWriteResult Import(string? computer, string? device)
    {
        Imports.Add((computer, device));
        return ImportResult;
    }

    public readonly List<(string? Computer, string? Device)> Selections = new List<(string? Computer, string? Device)>();

    /// <summary>The result selections return.</summary>
    public IcWriteResult SelectResult { get; set; } = new IcWriteResult(true, "Light Controller", 0, null);

    public IcWriteResult Select(string? computer, string? device)
    {
        Selections.Add((computer, device));
        return SelectResult;
    }

    /// <summary>The selection reads return.</summary>
    public ReadComputerResult SelectionResult { get; set; } = new ReadComputerResult(null, null, null, null);

    public ReadComputerResult Selected(string? computer)
    {
        return SelectionResult;
    }
}
