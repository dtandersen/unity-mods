using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: describe one live device in full.</summary>
    public sealed class DescribeDeviceCommand
    {
        private readonly IDeviceInspector _inspector;

        public DescribeDeviceCommand(IDeviceInspector inspector)
        {
            _inspector = inspector;
        }

        public DeviceDetails Execute(string device)
        {
            return _inspector.Describe(device);
        }
    }
}
