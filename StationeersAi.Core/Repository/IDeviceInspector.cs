using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>Full details of one live device.</summary>
    public interface IDeviceInspector
    {
        /// <summary>Details for the named device, or null when no single device matches.</summary>
        DeviceDetails Describe(string device);
    }
}
