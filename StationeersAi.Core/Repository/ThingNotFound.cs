using System;

namespace StationeersAi.Repository;

/// <summary>
/// Says no thing carries the given id. Repositories raise it from a deep
/// guard and catch it at the boundary, so the guard reads as a refusal.
/// <see cref="Describe"/> gives the same sentence to lookups that return
/// their error instead of throwing it.
/// </summary>
public sealed class ThingNotFound : Exception
{
    /// <param name="kind">What was sought, as the stationeer names it: "Computer" or "Housing".</param>
    /// <param name="thingId">The thing id handed in.</param>
    /// <param name="where">Where it was sought, such as " on this computer's network", or null.</param>
    /// <param name="available">The things it could have been, already joined, or null to leave them out.</param>
    public ThingNotFound(string kind, long thingId, string? where = null, string? available = null)
        : base(Describe(kind, thingId, where, available))
    {
    }

    /// <summary>The sentence for one missed lookup.</summary>
    public static string Describe(string kind, long thingId, string? where = null, string? available = null)
    {
        var message = $"{kind} {thingId} not found{where}.";
        return available == null ? message : message + $" Available: {available}";
    }
}
