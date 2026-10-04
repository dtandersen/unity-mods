using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: find live logic devices.</summary>
public sealed class SearchDevicesCommand
{
    private readonly IDeviceCatalog _devices;

    public SearchDevicesCommand(IDeviceCatalog devices)
    {
        _devices = devices;
    }

    public IReadOnlyList<DeviceInfo> Execute(string? query, long? network, bool? logic, long? room)
    {
        return _devices.Search(query, network, logic, room);
    }
}
