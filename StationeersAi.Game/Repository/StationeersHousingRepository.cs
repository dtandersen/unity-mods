using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Finds the live housing a label or thing id names and builds one
/// <see cref="StationeersHousing"/> for the single match. Reads Unity's
/// objects on the main thread.
/// </summary>
public sealed class StationeersHousingRepository : IHousingRepository
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersHousingRepository(IMainThread threads, Logger log)
    {
        _threads = threads;
        _log = log;
    }

    public HousingLookup Find(string? device)
    {
        try
        {
            return _threads.RunSync(() => FindInner(device));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new HousingLookup(null, "Housing lookup failed.");
        }
    }

    private HousingLookup FindInner(string? device)
    {
        var housings = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<CircuitHousing>(),
            housing => Name(housing),
            housing => Id(housing),
            device);
        if (housings.Count == 0)
        {
            return new HousingLookup(null, $"Housing '{device}' not found.");
        }

        if (housings.Count > 1)
        {
            return new HousingLookup(null, $"Several housings match '{device}'.");
        }

        return new HousingLookup(new StationeersHousing(housings[0], _threads, _log), null);
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

    private static long Id(CircuitHousing housing)
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
