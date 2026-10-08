using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Ask for devices by name fragment, network, logic, and room.</summary>
public sealed record SearchDevicesRequest(string? Query, long? Network, bool? Logic, long? Room);

/// <summary>The devices that matched; empty when none did.</summary>
public sealed record DevicesResult(
    Status Status, List<string> Errors, IReadOnlyList<DeviceInfo> Devices) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the device catalogue.</summary>
public sealed class SearchDevicesCommand : Command<SearchDevicesRequest, DevicesResult>
{
    private readonly IDeviceCatalog _devices;

    public SearchDevicesCommand(IDeviceCatalog devices)
    {
        _devices = devices;
    }

    public DevicesResult Execute(SearchDevicesRequest request)
    {
        return new DevicesResult(
            Status.SUCCESS,
            new List<string>(),
            _devices.Search(request.Query, request.Network, request.Logic, request.Room));
    }
}
