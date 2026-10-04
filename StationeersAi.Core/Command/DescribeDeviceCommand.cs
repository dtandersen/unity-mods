using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;
using Caev.Tools;

namespace StationeersAi.Command;

/// <summary>Domain operation: describe one live device in full.</summary>
public sealed class DescribeDeviceCommand
{
    private readonly IDeviceInspector _inspector;
    private readonly IDeviceCatalog _catalog;

    public DescribeDeviceCommand(IDeviceInspector inspector, IDeviceCatalog catalog)
    {
        _inspector = inspector;
        _catalog = catalog;
    }

    public DeviceDetails? Execute(string? device)
    {
        return _inspector.Describe(device);
    }

    /// <summary>Brief matches for a device no single entry fits, most recently added last.</summary>
    public IReadOnlyList<DeviceInfo> Candidates(string? device)
    {
        return _catalog.Search(device, null, null, null).Take(ToolLimits.MaxResults).ToArray();
    }
}
