using System;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Core;
using StationeersAi.Game;
using StationeersAi.Entity;
using StationeersAi.Repository;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Repository;

/// <summary>Reads burned chip code from housings by label or thing id.</summary>
public sealed class StationeersIcHousing : IIcHousing
{
    private readonly IMainThread _threads;
    private readonly Logger _log;

    public StationeersIcHousing(IMainThread threads, Logger log)
    {
        _threads = threads;
        _log = log;
    }

    public IcReadResult Read(string? device)
    {
        try
        {
            return _threads.RunSync(() => ReadInner(device));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcReadResult(false, null, null, "Housing read failed.");
        }
    }

    private static IcReadResult ReadInner(string? device)
    {
        var housings = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<CircuitHousing>(),
            housing => SafeName(housing),
            housing => SafeId(housing),
            device);
        if (housings.Count == 0)
        {
            return new IcReadResult(false, null, null, $"Housing '{device}' not found.");
        }

        if (housings.Count > 1)
        {
            return new IcReadResult(false, null, null, $"Several housings match '{device}'.");
        }

        var housing = housings[0];
        string code;
        try
        {
            var chip = housing._ProgrammableChipSlot?.Get<ProgrammableChip>();
            var source = chip?.GetSourceCode();
            code = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception)
        {
            return new IcReadResult(false, null, null, "Housing read failed.");
        }

        return new IcReadResult(true, SafeName(housing), code, null);
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
