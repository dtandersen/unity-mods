using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Built things in the world: devices, pipes, walls.</summary>
public interface IDeviceCatalog
{
    /// <summary>
    /// Things matching <paramref name="query"/> (blank matches all), on data
    /// network <paramref name="network"/> (null searches every network), speaking
    /// logic when <paramref name="logic"/> is true, dumb ones when false, all
    /// when null, and in room <paramref name="room"/> (null searches everywhere).
    /// </summary>
    IReadOnlyList<DeviceInfo> Search(string query, long? network, bool? logic, long? room);
}
