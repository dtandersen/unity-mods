using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;
using Caev.Tools;

namespace StationeersAi.Command;

/// <summary>Ask for one device, by label or reference id.</summary>
public sealed record DescribeDeviceRequest(string? Device);

/// <summary>
/// One device's full description, or the brief matches when no single entry
/// fits. <see cref="Candidates"/> stays empty on a good read and when
/// nothing matches at all.
/// </summary>
public sealed record DeviceResult(
    Status Status,
    List<string> Errors,
    DeviceDetails? Details,
    IReadOnlyList<DeviceInfo> Candidates) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: describe one live device. A label or id no single
/// device answers to raises <see cref="Status.NOT_FOUND"/> with the brief
/// matches that came closest.
/// </summary>
public sealed class DescribeDeviceCommand : Command<DescribeDeviceRequest, DeviceResult>
{
    private readonly IDeviceInspector _inspector;
    private readonly IDeviceCatalog _catalog;

    public DescribeDeviceCommand(IDeviceInspector inspector, IDeviceCatalog catalog)
    {
        _inspector = inspector;
        _catalog = catalog;
    }

    public DeviceResult Execute(DescribeDeviceRequest request)
    {
        var details = _inspector.Describe(request.Device);
        if (details != null)
        {
            return new DeviceResult(Status.SUCCESS, new List<string>(), details, Array.Empty<DeviceInfo>());
        }

        var candidates = _catalog.Search(request.Device, null, null, null).Take(ToolLimits.MaxResults).ToArray();
        return new DeviceResult(
            Status.NOT_FOUND,
            new List<string> { $"No single device matches '{request.Device}'." },
            null,
            candidates);
    }
}
