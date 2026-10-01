using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>Live logic devices in the world.</summary>
    public interface IDeviceCatalog
    {
        /// <summary>
        /// Devices matching <paramref name="query"/> (blank matches all),
        /// limited to one computer's data network when <paramref name="computer"/>
        /// names one (null searches the whole world).
        /// </summary>
        IReadOnlyList<DeviceInfo> Search(string query, string computer);
    }
}
