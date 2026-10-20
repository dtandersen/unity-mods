using System;
using System.Collections.Generic;
using Sage.Core;
using Sage.Game;
using Logger = Crowbus.Common.Logger;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Sage.Entity;
using Sage.Repository;
using UnityEngine;

namespace Sage.Repository;

/// <summary>
/// Describes one live thing, structure or loose. A structure match carries
/// its full device payload via the device inspector; a loose match carries
/// identity, quantity, and position. Snapshotted on Unity's main thread;
/// dying things are skipped, never fatal.
/// </summary>
public sealed class StationeersThingInspector : IThingInspector
{
    private readonly IMainThread _threads;
    private readonly IStructureInspector _devices;
    private readonly Logger _log;

    public StationeersThingInspector(IMainThread mainThread, IStructureInspector devices, Logger log)
    {
        _threads = mainThread;
        _devices = devices;
        _log = log;
    }

    public ThingDetails? Describe(string? thing)
    {
        try
        {
            return _threads.RunSync(() => DescribeInner(thing));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return null;
        }
    }

    private ThingDetails? DescribeInner(string? thing)
    {
        var matches = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<Thing>(),
            item => SafeName(item),
            item => SafeId(item),
            thing);
        if (matches.Count != 1)
        {
            return null;
        }

        var target = matches[0];
        if (target is Assets.Scripts.Objects.Structure structure)
        {
            return DescribeStructure(structure);
        }

        if (target is DynamicThing loose)
        {
            return DescribeLoose(loose);
        }

        return null;
    }

    private ThingDetails? DescribeStructure(Assets.Scripts.Objects.Structure structure)
    {
        try
        {
            var details = _devices.Describe(structure.ReferenceId.ToString());
            if (details == null)
            {
                return null;
            }

            var center = SafeCenter(structure);
            return new ThingDetails(
                SafeName(structure),
                SafePrefab(structure),
                SafeId(structure),
                "structure",
                null,
                new StructurePosition(center.x, center.y, center.z),
                RoomOf(structure),
                details.Connections,
                structure is ILogicable,
                details);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static ThingDetails? DescribeLoose(DynamicThing loose)
    {
        try
        {
            var center = SafeCenter(loose);
            return new ThingDetails(
                SafeName(loose),
                SafePrefab(loose),
                SafeId(loose),
                "thing",
                QuantityText(loose),
                new StructurePosition(center.x, center.y, center.z),
                RoomOf(loose),
                new List<NetworkConnection>(),
                false,
                null);
        }
        catch (Exception)
        {
            return null;
        }
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
            if (dynamicThing != null)
            {
                var room = dynamicThing.Room;
                return room != null ? room.RoomId : 0;
            }

            var controller = RoomController.World;
            if (controller == null)
            {
                return 0;
            }

            var built = controller.GetRoom(thing.GridPosition);
            return built != null ? built.RoomId : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static Vector3 SafeCenter(Thing thing)
    {
        try
        {
            return thing != null ? thing.CenterPosition : Vector3.zero;
        }
        catch (Exception)
        {
            return Vector3.zero;
        }
    }

    private static string? SafeName(Thing thing)
    {
        try
        {
            return thing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafePrefab(Thing thing)
    {
        try
        {
            return thing.PrefabName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeId(Thing thing)
    {
        try
        {
            return thing.ReferenceId;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
