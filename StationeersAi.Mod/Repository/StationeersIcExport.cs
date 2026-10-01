using System;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using StationeersAi.Mod;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>
/// Burns a computer's editor text onto a housing chip, then verifies the
/// burn by comparing code hashes. Runs on Unity's main thread: the burn
/// compiles on the chip and touches Unity state.
/// </summary>
public sealed class StationeersIcExport : IIcExport
{
    private readonly IMainThread _threads;

    private readonly BepinExLogger _log;

    public StationeersIcExport(IMainThread mainThread, BepinExLogger log)
    {
        _threads = mainThread;
        _log = log;
    }
    public IcWriteResult Export(string computer, string device)
    {
        try
        {
            return _threads.RunSync(() => ExportInner(computer, device));
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }
    }

    private IcWriteResult ExportInner(string computer, string device)
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
            _log.Log(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor read failed.");
        }

        if (string.IsNullOrEmpty(text))
        {
            return new IcWriteResult(false, null, 0, "Editor is empty; nothing to export.");
        }

        var housings = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<CircuitHousing>(),
            housing => SafeName(housing),
            housing => SafeId(housing),
            device);
        if (housings.Count == 0)
        {
            return new IcWriteResult(false, null, 0, $"Housing '{device}' not found.");
        }

        if (housings.Count > 1)
        {
            return new IcWriteResult(false, null, 0, $"Several housings match '{device}'.");
        }

        var housing = housings[0];
        try
        {
            housing.SetSourceCode(text);
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }

        var chip = HousingChip(housing);
        if (chip != null && chip.CompilationError)
        {
            var type = ChipErrorReading.CompileErrorType(chip) ?? "compile error";
            var line = ChipErrorReading.CompileErrorLine(chip);
            return new IcWriteResult(false, null, 0, $"Chip reports {type}" + (line == null ? "." : $" at line {line}."));
        }

        string burned = null;
        try
        {
            if (chip != null)
            {
                burned = chip.GetSourceCode().ToString();
            }
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
        }

        if (burned == null || CodeHash.Hash(burned) != CodeHash.Hash(text))
        {
            return new IcWriteResult(false, null, 0, "Chip hash does not match the editor.");
        }

        return new IcWriteResult(true, SafeName(housing) ?? "housing", text.Split('\n').Length, null);
    }

    private static ProgrammableChip HousingChip(CircuitHousing housing)
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

    private static string SafeName(CircuitHousing housing)
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
