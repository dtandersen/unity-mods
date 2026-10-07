using System;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Core;
using StationeersAi.Formatting;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Entity;

/// <summary>
/// One live IC housing, built by <see cref="StationeersIcDevices"/>
/// from the housing it names. Reads the program burned on the chip it holds.
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
        return new Program(code ?? string.Empty, CodeHash.Hash(code));
    }
}
