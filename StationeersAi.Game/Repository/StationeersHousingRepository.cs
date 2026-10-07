using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Electrical;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Finds the live housing carrying a thing id and builds one
/// <see cref="StationeersHousing"/> for it. Reads Unity's objects on the main
/// thread.
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

    public IHousing Find(long thingId)
    {
        try
        {
            return _threads.RunSync(() => FindInner(thingId));
        }
        catch (Exception ex) when (!(ex is ThingNotFound))
        {
            // A game fault, not a missed id: record it, then let the caller report it.
            _log.Info(ex.ToString());
            throw;
        }
    }

    private IHousing FindInner(long thingId)
    {
        var housings = All();
        foreach (var housing in housings)
        {
            long id;
            try
            {
                id = housing.ReferenceId;
            }
            catch (Exception)
            {
                continue;
            }

            if (id == thingId)
            {
                return new StationeersHousing(housing, _threads, _log);
            }
        }

        throw new ThingNotFound("Housing", thingId, null, Described(housings));
    }

    private static List<CircuitHousing> All()
    {
        var housings = new List<CircuitHousing>();
        foreach (var housing in UnityEngine.Object.FindObjectsOfType<CircuitHousing>())
        {
            if (housing != null)
            {
                housings.Add(housing);
            }
        }

        return housings;
    }

    /// <summary>The housings in the world, each labelled and numbered for a missed lookup.</summary>
    private static string Described(IReadOnlyList<CircuitHousing> housings)
    {
        var listed = new List<string>();
        foreach (var housing in housings)
        {
            string entry;
            try
            {
                entry = $"{housing.DisplayName} #{housing.ReferenceId}";
            }
            catch (Exception)
            {
                continue;
            }

            if (!listed.Contains(entry))
            {
                listed.Add(entry);
            }
        }

        return listed.Count == 0 ? "none" : string.Join(", ", listed.ToArray());
    }
}
