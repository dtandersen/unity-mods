using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

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

    /// <summary>The selection reads return.</summary>
    public ReadComputerResult SelectionResult { get; set; } = new ReadComputerResult(null, null, null, null);

    public IcWriteResult Write(string code)
    {
        Writes.Add(code);
        return new IcWriteResult(true, Name, (code ?? string.Empty).Split('\n').Length, null);
    }

    public IcReadResult Read()
    {
        return new IcReadResult(true, Name, EditorCode, null);
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

    public ReadComputerResult Selected()
    {
        return SelectionResult;
    }
}
