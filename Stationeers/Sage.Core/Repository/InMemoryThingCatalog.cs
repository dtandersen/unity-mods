using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory thing catalog. Used by tests.</summary>
public sealed class InMemoryThingCatalog : IThingCatalog
{
    public List<ThingInfo> Things { get; } = new List<ThingInfo>();

    public IReadOnlyList<ThingInfo> Search(string? query, Point? origin, double? maxDistanceM)
    {
        return Things
            .Where(thing =>
                (string.IsNullOrWhiteSpace(query) ||
                (thing.Name != null && thing.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (thing.PrefabName != null && thing.PrefabName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                (origin == null || !maxDistanceM.HasValue || Distance(thing, origin) <= maxDistanceM.Value))
            .ToArray();
    }

    private static double Distance(ThingInfo thing, Point origin)
    {
        var dx = thing.X - origin.X;
        var dy = thing.Y - origin.Y;
        var dz = thing.Z - origin.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
