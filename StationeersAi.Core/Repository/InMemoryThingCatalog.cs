using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory thing catalog. Used by tests.</summary>
public sealed class InMemoryThingCatalog : IThingCatalog
{
    public List<ThingInfo> Things { get; } = new List<ThingInfo>();

    public IReadOnlyList<ThingInfo> Search(string? query, OriginPoint? origin, double? maxDistanceM)
    {
        return Things
            .Where(thing =>
                (string.IsNullOrWhiteSpace(query) ||
                (thing.Name != null && thing.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (thing.Type != null && thing.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                (origin == null || !maxDistanceM.HasValue || Distance(thing, origin) <= maxDistanceM.Value))
            .ToArray();
    }

    private static double Distance(ThingInfo thing, OriginPoint origin)
    {
        var dx = thing.X - origin.X;
        var dy = thing.Y - origin.Y;
        var dz = thing.Z - origin.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
