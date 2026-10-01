using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Live logic devices matching a search.</summary>
    public sealed record SearchDevicesResult(IReadOnlyList<DeviceInfo> Devices);
}
