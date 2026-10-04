using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using StationeersAi.Entity;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>
/// Finds built things: devices, pipes, walls. Logic speakers are the ones
/// implementing <c>ILogicable</c>. Snapshotted on Unity's main thread;
/// dying things are skipped, never fatal.
/// </summary>
public sealed class StationeersDeviceCatalog : IDeviceCatalog
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersDeviceCatalog(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }
    public IReadOnlyList<DeviceInfo> Search(string? query, long? network, bool? logic, long? room)
    {
        try
        {
            return _threads.RunSync(() => SearchInner(query, network, logic, room));
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return new DeviceInfo[0];
        }
    }

    private IReadOnlyList<DeviceInfo> SearchInner(string? query, long? network, bool? logic, long? room)
    {
        var devices = new List<DeviceInfo>();
        foreach (var structure in UnityEngine.Object.FindObjectsOfType<Assets.Scripts.Objects.Structure>())
        {
            var info = Describe(structure);
            if (info != null &&
                Matches(info, query) &&
                (!network.HasValue || info.NetworkId == network.Value) &&
                (!logic.HasValue || info.Logic == logic.Value) &&
                (!room.HasValue || info.RoomId == room.Value))
            {
                devices.Add(info);
            }
        }

        return devices;
    }

    private static DeviceInfo? Describe(Assets.Scripts.Objects.Structure structure)
    {
        try
        {
            if (structure == null)
            {
                return null;
            }

            long network = 0;
            try
            {
                var device = structure as Device;
                var networkObject = device != null ? device.DataCableNetwork : null;
                network = networkObject != null ? networkObject.ReferenceId : 0;
            }
            catch (Exception)
            {
            }

            return DescribeThing(structure, network, structure is ILogicable, RoomOf(structure));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long RoomOf(Thing thing)
    {
        try
        {
            var controller = RoomController.World;
            if (controller == null || thing == null)
            {
                return 0;
            }

            var room = controller.GetRoom(thing.GridPosition);
            return room != null ? room.RoomId : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static DeviceInfo? DescribeThing(Thing thing, long network, bool logic, long room)
    {
        try
        {
            return new DeviceInfo(thing.DisplayName, thing.PrefabName, thing.ReferenceId, network, logic, room);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool Matches(DeviceInfo info, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return (info.Name != null && info.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
            (info.Type != null && info.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
