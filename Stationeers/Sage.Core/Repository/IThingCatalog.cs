using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>Finds loose world things. A null origin searches everywhere.</summary>
public interface IThingCatalog
{
    IReadOnlyList<ThingInfo> Search(string? query, Point? origin, double? maxDistanceM);
}
