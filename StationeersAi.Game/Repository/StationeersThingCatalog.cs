using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using StationeersAi.Entity;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>
/// Finds loose world things: ores, ingots, plants, dropped items, wreckage.
/// Enumerates dynamic things, skipping players, animals, and carried items;
/// structures stay with the device catalog. Snapshotted on Unity's main
/// thread; dying things are skipped, never fatal.
/// </summary>
public sealed class StationeersThingCatalog : IThingCatalog
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersThingCatalog(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }

    public IReadOnlyList<ThingInfo> Search(string? query, Point? origin, double? maxDistanceM)
    {
        try
        {
            return _threads.RunSync(() => SearchInner(query, origin, maxDistanceM));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new ThingInfo[0];
        }
    }

    private static IReadOnlyList<ThingInfo> SearchInner(string? query, Point? origin, double? maxDistanceM)
    {
        var originVector = origin != null
            ? new Vector3((float)origin.X, (float)origin.Y, (float)origin.Z)
            : (Vector3?)null;

        var found = new List<ThingInfo>();
        foreach (var thing in UnityEngine.Object.FindObjectsOfType<DynamicThing>())
        {
            try
            {
                if (thing == null || thing is Assets.Scripts.Objects.Entity || thing.RootParentHuman != null)
                {
                    continue;
                }

                var name = thing.DisplayName ?? thing.PrefabName;
                if (name == null)
                {
                    continue;
                }

                if (query is not null && !string.IsNullOrWhiteSpace(query) &&
                    (name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    (thing.PrefabName == null || thing.PrefabName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)))
                {
                    continue;
                }

                var center = thing.CenterPosition;
                if (originVector.HasValue && maxDistanceM.HasValue &&
                    Vector3.Distance(center, originVector.Value) > maxDistanceM.Value)
                {
                    continue;
                }

                found.Add(new ThingInfo(
                    name,
                    thing.PrefabName,
                    thing.ReferenceId,
                    QuantityText(thing),
                    center.x,
                    center.y,
                    center.z,
                    RoomOf(thing)));
            }
            catch (Exception)
            {
            }
        }

        return found;
    }

    private static string? QuantityText(DynamicThing thing)
    {
        if (thing is IQuantity quantity)
        {
            try
            {
                return quantity.GetQuantityText();
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    private static long RoomOf(Thing thing)
    {
        try
        {
            var dynamicThing = thing as DynamicThing;
            if (dynamicThing == null)
            {
                return 0;
            }

            var room = dynamicThing.Room;
            return room != null ? room.RoomId : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
