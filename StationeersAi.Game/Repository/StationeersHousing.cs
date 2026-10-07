using System;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Core;
using StationeersAi.Entity;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

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

    public IcReadResult Read()
    {
        try
        {
            return _threads.RunSync(ReadInner);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcReadResult(false, null, null, "Housing read failed.");
        }
    }

    private IcReadResult ReadInner()
    {
        string code;
        try
        {
            var chip = _housing._ProgrammableChipSlot?.Get<ProgrammableChip>();
            var source = chip?.GetSourceCode();
            code = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception)
        {
            return new IcReadResult(false, null, null, "Housing read failed.");
        }

        return new IcReadResult(true, Name(_housing), code, null);
    }

    private static string? Name(CircuitHousing housing)
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
}
