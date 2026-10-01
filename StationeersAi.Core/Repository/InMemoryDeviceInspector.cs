using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory device inspector. Used by tests.</summary>
    public sealed class InMemoryDeviceInspector : IDeviceInspector
    {
        public DeviceDetails Details { get; set; }

        public string LastDevice { get; private set; }

        public DeviceDetails Describe(string device)
        {
            LastDevice = device;
            return Details;
        }
    }
}
