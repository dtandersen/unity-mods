using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory device catalog. Used by tests.</summary>
public sealed class InMemoryDeviceCatalog : IDeviceCatalog
{
    public List<DeviceInfo> Devices { get; } = new List<DeviceInfo>();

    public long? LastNetwork { get; private set; }

    public bool? LastLogic { get; private set; }

    public long? LastRoom { get; private set; }

    public IReadOnlyList<DeviceInfo> Search(string? query, long? network, bool? logic, long? room)
    {
        LastNetwork = network;
        LastLogic = logic;
        LastRoom = room;
        return Devices
            .Where(device =>
                (!network.HasValue || device.NetworkId == network.Value) &&
                (!logic.HasValue || device.Logic == logic.Value) &&
                (!room.HasValue || device.RoomId == room.Value) &&
                (string.IsNullOrWhiteSpace(query) ||
                (device.Name != null && device.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (device.PrefabName != null && device.PrefabName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)))
            .ToArray();
    }
}
