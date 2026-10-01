using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>One live device in full: ports, settings, and slots.</summary>
    public sealed record DescribeDeviceResult(DeviceDetails Details);
}
