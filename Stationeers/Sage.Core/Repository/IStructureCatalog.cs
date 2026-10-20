using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>Built structures in the world: machines, pipes, walls.</summary>
public interface IStructureCatalog
{
    /// <summary>
    /// Things matching <paramref name="query"/> (blank matches all), on power
    /// network <paramref name="powerNetworkId"/> and data network
    /// <paramref name="dataNetworkId"/> (each null searches every network),
    /// speaking logic when <paramref name="logic"/> is true, dumb ones when
    /// false, all when null, and in room <paramref name="room"/> (null
    /// searches everywhere).
    /// </summary>
    IReadOnlyList<StructureInfo> Search(string? query, long? powerNetworkId, long? dataNetworkId, bool? logic, long? room);
}
