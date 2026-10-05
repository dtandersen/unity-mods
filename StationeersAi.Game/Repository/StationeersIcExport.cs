using System;
using System.Collections.Generic;
using System.Reflection;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>
/// Burns a computer's editor text onto a housing chip through the computer
/// motherboard, the same path the monitor Export button takes. The target
/// must sit in the motherboard's holder list, so only housings on the
/// computer's network can receive the burn. Verifies by comparing code hashes.
/// Runs on Unity's main thread: the burn compiles on the chip and touches
/// Unity state.
/// </summary>
public sealed class StationeersIcExport : IIcExport
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersIcExport(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }
    public IcWriteResult Export(string? computer, string? device)
    {
        try
        {
            return _threads.RunSync(() => ExportInner(computer, device));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }
    }

    public IcWriteResult Import(string? computer, string? device)
    {
        try
        {
            return _threads.RunSync(() => ImportInner(computer, device));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Import failed.");
        }
    }

    private IcWriteResult ImportInner(string? computer, string? device)
    {
        var boards = ComputerBoards.Match(computer);
        if (boards.Count == 0)
        {
            return new IcWriteResult(false, null, 0, $"Computer '{computer}' not found. Available: {ComputerBoards.Available()}");
        }

        if (boards.Count > 1)
        {
            return new IcWriteResult(false, null, 0, $"Several computers match '{computer}': {ComputerBoards.Names(boards)}");
        }

        var motherboard = boards[0].Motherboard;
        var selection = MatchNetworked(motherboard, device);
        if (selection.Error != null)
        {
            return selection.Error;
        }

        try
        {
            SelectHolder(motherboard, selection.Index);
            motherboard.Import();
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Import failed.");
        }

        string text;
        try
        {
            var source = motherboard.GetSourceCode();
            text = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor read failed.");
        }

        return new IcWriteResult(true, boards[0].Name, text.Split('\n').Length, null);
    }

    /// <summary>
    /// Matches <paramref name="device"/> inside the motherboard's holder
    /// list: only housings on the computer's network. Carries the holder
    /// index for the dropdown and an error when nothing matches.
    /// </summary>
    private sealed class NetworkSelection
    {
        public int Index;
        public CircuitHousing? Housing;
        public IcWriteResult? Error;
    }

    private NetworkSelection MatchNetworked(object motherboard, string? device)
    {
        var selection = new NetworkSelection();
        var holders = MotherboardHolders(motherboard);
        if (holders == null)
        {
            selection.Error = new IcWriteResult(false, null, 0, "Export failed.");
            return selection;
        }

        var networked = new List<CircuitHousing>();
        foreach (var holder in holders)
        {
            var housing = holder as CircuitHousing;
            if (housing != null)
            {
                networked.Add(housing);
            }
        }

        if (networked.Count == 0)
        {
            selection.Error = new IcWriteResult(false, null, 0, "No housings on this computer's network.");
            return selection;
        }

        var matches = ThingMatching.Match(
            networked,
            housing => SafeName(housing),
            housing => SafeId(housing),
            device);
        if (matches.Count == 0)
        {
            selection.Error = new IcWriteResult(false, null, 0, $"Housing '{device}' not found on this computer's network. Available: {HousingNames(networked)}");
            return selection;
        }

        if (matches.Count > 1)
        {
            selection.Error = new IcWriteResult(false, null, 0, $"Several housings match '{device}'.");
            return selection;
        }

        selection.Index = holders.IndexOf(matches[0]);
        selection.Housing = matches[0];
        return selection;
    }

    private IcWriteResult ExportInner(string? computer, string? device)
    {
        var boards = ComputerBoards.Match(computer);
        if (boards.Count == 0)
        {
            return new IcWriteResult(false, null, 0, $"Computer '{computer}' not found. Available: {ComputerBoards.Available()}");
        }

        if (boards.Count > 1)
        {
            return new IcWriteResult(false, null, 0, $"Several computers match '{computer}': {ComputerBoards.Names(boards)}");
        }

        string text;
        try
        {
            var source = boards[0].Motherboard.GetSourceCode();
            text = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor read failed.");
        }

        if (string.IsNullOrEmpty(text))
        {
            return new IcWriteResult(false, null, 0, "Editor is empty; nothing to export.");
        }

        var motherboard = boards[0].Motherboard;
        var selection = MatchNetworked(motherboard, device);
        if (selection.Error != null)
        {
            return selection.Error;
        }

        var target = selection.Housing;
        if (target == null)
        {
            return new IcWriteResult(false, null, 0, "Export failed.");
        }

        try
        {
            SelectHolder(motherboard, selection.Index);
            motherboard.Export();
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }

        var chip = HousingChip(target);
        if (chip != null && chip.CompilationError)
        {
            var type = ChipErrorReading.CompileErrorType(chip) ?? "compile error";
            var line = ChipErrorReading.CompileErrorLine(chip);
            return new IcWriteResult(false, null, 0, $"Chip reports {type}" + (line == null ? "." : $" at line {line}."));
        }

        string? burned = null;
        try
        {
            if (chip != null)
            {
                burned = chip.GetSourceCode().ToString();
            }
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
        }

        if (burned == null || CodeHash.Hash(burned) != CodeHash.Hash(text))
        {
            return new IcWriteResult(false, null, 0, "Chip hash does not match the editor.");
        }

        return new IcWriteResult(true, SafeName(target) ?? "housing", text.Split('\n').Length, null);
    }

    /// <summary>
    /// The motherboard's holder list: only housings on the computer's
    /// network. Read by reflection; the game keeps it private.
    /// </summary>
    private static IList<ICircuitHolder>? MotherboardHolders(object motherboard)
    {
        try
        {
            var field = motherboard.GetType().GetField("_circuitHolders", BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(motherboard) as IList<ICircuitHolder>;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Points the motherboard dropdown at one holder by index.</summary>
    private void SelectHolder(object motherboard, int index)
    {
        var field = motherboard.GetType().GetField("_dropdown", BindingFlags.Instance | BindingFlags.NonPublic);
        var dropdown = field?.GetValue(motherboard);
        if (dropdown == null)
        {
            throw new InvalidOperationException("Motherboard dropdown is missing.");
        }

        dropdown.GetType().GetMethod("SetSelectedIndex")?.Invoke(dropdown, new object[] { index });
    }

    private static string HousingNames(IReadOnlyList<CircuitHousing> housings)
    {
        var names = new List<string>();
        foreach (var housing in housings)
        {
            var name = SafeName(housing);
            if (!string.IsNullOrWhiteSpace(name) && !names.Contains(name))
            {
                names.Add(name);
            }
        }

        return names.Count == 0 ? "none" : string.Join(", ", names.ToArray());
    }

    private static ProgrammableChip? HousingChip(CircuitHousing housing)
    {
        try
        {
            return housing?._ProgrammableChipSlot?.Get<ProgrammableChip>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafeName(CircuitHousing housing)
    {
        try
        {
            return housing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeId(CircuitHousing housing)
    {
        try
        {
            return housing.ReferenceId;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
