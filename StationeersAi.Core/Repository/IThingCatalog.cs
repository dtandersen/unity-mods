using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Finds loose world things. A null origin searches everywhere.</summary>
public interface IThingCatalog
{
    IReadOnlyList<ThingInfo> Search(string? query, OriginPoint? origin, double? maxDistanceM);
}
