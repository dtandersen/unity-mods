using System.Collections.Generic;
using System.Linq;
using Sage.Entity;

namespace Sage.Formatting;

/// <summary>One-line rendering of a structure's connection list.</summary>
public static class NetworkConnections
{
    public static string Format(IReadOnlyList<NetworkConnection>? connections)
    {
        if (connections == null || connections.Count == 0)
        {
            return "none";
        }

        return string.Join(
            " ",
            connections
                .OrderBy(c => Order(c.Kind))
                .ThenBy(c => c.Index)
                .ThenBy(c => c.NetworkId)
                .Select(Render));
    }

    private static int Order(string kind)
    {
        switch (kind)
        {
            case "power": return 0;
            case "data": return 1;
            case "pipe": return 2;
            case "chute": return 3;
            default: return 4;
        }
    }

    private static string Render(NetworkConnection c)
    {
        switch (c.Kind)
        {
            case "power": return $"p{c.Index}:{c.NetworkId}";
            case "data": return $"d{c.Index}:{c.NetworkId}";
            case "pipe": return $"{(c.Port ?? "pipe")}:{c.NetworkId}";
            case "chute": return $"c{c.Index}:{c.NetworkId}";
            default: return $"{c.Kind}{c.Index}:{c.NetworkId}";
        }
    }
}
