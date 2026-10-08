using System;
using System.Collections.Generic;
using StationeersAi.Formatting;
using StationeersAi.Repository;
using StationeersAi.Repository;

namespace StationeersAi.Entity;

/// <summary>
/// In-memory IC computer: one screen, one dropdown, one pair of buttons.
/// The buttons act on <see cref="NetworkedHousing"/> the way the game's do,
/// so reading the entities afterwards tells the truth about a burn.
/// </summary>
public sealed class InMemoryComputer : IComputer
{
    public readonly List<string?> Writes = new List<string?>();

    public readonly List<long> Exports = new List<long>();

    public readonly List<long> Selections = new List<long>();

    /// <summary>The name reads and write answers report as the computer.</summary>
    public string Name { get; set; } = "Lab Computer";

    /// <summary>The text reads return as the screen contents.</summary>
    public string EditorCode { get; set; } = string.Empty;

    /// <summary>The housing on this computer's network, or null when it holds none.</summary>
    public InMemoryHousing? NetworkedHousing { get; set; }

    /// <summary>The name of the housing the dropdown has selected, or null when it selects none.</summary>
    public string? SelectedDevice { get; set; } = "Climate Control";

    /// <summary>The thing id of the housing the dropdown has selected (0 when none).</summary>
    public long SelectedThing { get; set; } = 2200;

    public void Write(string code)
    {
        Writes.Add(code);
        EditorCode = code ?? string.Empty;
    }

    public Program Read()
    {
        if (ReadFault != null)
        {
            throw ReadFault;
        }

        return new Program(EditorCode, CodeHash.Hash(EditorCode));
    }

    /// <summary>Set false so exports leave the chip untouched: a burn that silently fails.</summary>
    public bool BurnTakes { get; set; } = true;

    /// <summary>Set to make reads raise, the way a broken editor would.</summary>
    public Exception? ReadFault { get; set; }

    public void Export(long device)
    {
        Exports.Add(device);
        var housing = OnNetwork(device);
        if (housing.CompileError == null && BurnTakes)
        {
            housing.ChipCode = EditorCode;
        }
    }

    public void Import()
    {
        var housing = NetworkedHousing;
        if (housing != null && SelectedThing == housing.Thing)
        {
            EditorCode = housing.ChipCode;
        }
    }

    public void Select(long device)
    {
        Selections.Add(device);
        var housing = OnNetwork(device);
        SelectedDevice = housing.Name;
        SelectedThing = housing.Thing;
    }

    public long Selected()
    {
        return string.IsNullOrEmpty(SelectedDevice) ? 0 : SelectedThing;
    }

    /// <summary>The networked housing, raising the game's own sentence when the id is not on it.</summary>
    private InMemoryHousing OnNetwork(long device)
    {
        var housing = NetworkedHousing;
        if (housing == null || housing.Thing != device)
        {
            var listed = housing == null ? "none" : $"{housing.Name} #{housing.Thing}";
            throw new ThingNotFound("Housing", device, " on this computer's network", listed);
        }

        return housing;
    }
}
