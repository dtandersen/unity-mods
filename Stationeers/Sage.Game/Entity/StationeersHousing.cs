using System;
using Assets.Scripts.Objects.Electrical;
using Sage.Core;
using Sage.Game;
using Sage.Repository;
using Logger = Sage.Core.Logger;

namespace Sage.Entity;

/// <summary>
/// One live IC housing, built by <see cref="StationeersIcDevices"/>
/// from the housing it names. Reads the program burned on the chip it holds,
/// its label, and what the chip reports against that program.
/// Runs on Unity's main thread: the chip belongs to Unity state.
/// </summary>
public sealed class StationeersHousing : IHousing
{
    private readonly CircuitHousing _housing;

    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersHousing(CircuitHousing housing, IMainThread threads, Logger log)
    {
        _housing = housing;
        _threads = threads;
        _log = log;
    }

    public string Name
    {
        get
        {
            try
            {
                return _threads.RunSync(() => ReadableName(_housing));
            }
            catch (Exception ex)
            {
                _log.Info(ex.ToString());
                return "housing";
            }
        }
    }

    public string? CompileError
    {
        get
        {
            try
            {
                return _threads.RunSync(ReadCompileError);
            }
            catch (Exception ex)
            {
                _log.Info(ex.ToString());
                throw;
            }
        }
    }

    public Program Read()
    {
        try
        {
            return _threads.RunSync(ReadInner);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            throw;
        }
    }

    private Program ReadInner()
    {
        var chip = _housing._ProgrammableChipSlot?.Get<ProgrammableChip>();
        var source = chip?.GetSourceCode();
        var code = source != null ? source.ToString() : string.Empty;
        return new Program(code ?? string.Empty, Formatting.CodeHash.Hash(code));
    }

    private string? ReadCompileError()
    {
        var chip = _housing._ProgrammableChipSlot?.Get<ProgrammableChip>();
        if (chip == null || !chip.CompilationError)
        {
            return null;
        }

        var type = ChipErrorReading.CompileErrorType(chip) ?? "compile error";
        var line = ChipErrorReading.CompileErrorLine(chip);
        return type + (line == null ? string.Empty : $" at line {line}");
    }

    private static string ReadableName(CircuitHousing housing)
    {
        try
        {
            return housing.DisplayName;
        }
        catch (Exception)
        {
            return "housing";
        }
    }
}
