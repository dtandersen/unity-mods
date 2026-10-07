using System;
using System.Collections.Generic;
using StationeersAi.Formatting;

namespace StationeersAi.Entity;

/// <summary>In-memory IC computer: one screen, one dropdown, one pair of buttons.</summary>
public sealed class InMemoryComputer : IComputer
{
    public readonly List<string?> Writes = new List<string?>();

    public readonly List<long> Exports = new List<long>();

    public readonly List<long> Imports = new List<long>();

    public readonly List<long> Selections = new List<long>();

    /// <summary>The name write and read results report as the computer.</summary>
    public string Name { get; set; } = "Lab Computer";

    /// <summary>The text reads return as the screen contents.</summary>
    public string EditorCode { get; set; } = string.Empty;

    /// <summary>The result exports return.</summary>
    public IcWriteResult Result { get; set; } = new IcWriteResult(true, "Climate Control", 37, null);

    /// <summary>The result imports return.</summary>
    public IcWriteResult ImportResult { get; set; } = new IcWriteResult(true, "Lab Computer", 3, null);

    /// <summary>The result selections return.</summary>
    public IcWriteResult SelectResult { get; set; } = new IcWriteResult(true, "Light Controller", 0, null);

    /// <summary>The housing the dropdown points at, or null when it points at none.</summary>
    public string? PointedDevice { get; set; } = "Climate Control";

    /// <summary>The thing id of the housing the dropdown points at (0 when none).</summary>
    public long PointedThing { get; set; } = 2200;

    /// <summary>The program the pointed housing's chip holds.</summary>
    public string PointedChipCode { get; set; } = string.Empty;

    public IcWriteResult Write(string code, string chipHash)
    {
        var refusal = WriteGuard.Refusal(PointedDevice, CodeHash.Hash(PointedChipCode), chipHash);
        if (refusal != null)
        {
            return new IcWriteResult(false, null, 0, refusal);
        }

        Writes.Add(code);
        return new IcWriteResult(true, Name, (code ?? string.Empty).Split('\n').Length, null);
    }

    /// <summary>Set to make reads raise, the way a broken editor would.</summary>
    public Exception? ReadFault { get; set; }

    public Program Read()
    {
        if (ReadFault != null)
        {
            throw ReadFault;
        }

        return new Program(EditorCode, CodeHash.Hash(EditorCode));
    }

    public IcWriteResult Export(long device)
    {
        Exports.Add(device);
        return Result;
    }

    public IcWriteResult Import(long device)
    {
        Imports.Add(device);
        return ImportResult;
    }

    public IcWriteResult Select(long device)
    {
        Selections.Add(device);
        return SelectResult;
    }

    public long Selected()
    {
        return string.IsNullOrEmpty(PointedDevice) ? 0 : PointedThing;
    }
}
