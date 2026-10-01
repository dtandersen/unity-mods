using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory device catalog. Used by tests.</summary>
    public sealed class InMemoryDeviceCatalog : IDeviceCatalog
    {
        public List<DeviceInfo> Devices { get; } = new List<DeviceInfo>();

        public string LastComputer { get; private set; }

        public IReadOnlyList<DeviceInfo> Search(string query, string computer)
        {
            LastComputer = computer;
            return Devices
                .Where(device =>
                    string.IsNullOrWhiteSpace(query) ||
                    (device.Name != null && device.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (device.Type != null && device.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToArray();
        }
    }
}
