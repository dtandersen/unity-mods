using System;

namespace StationeersAi.Repository;

/// <summary>
/// Says no thing carries the given id. Lookups raise it from their guard and
/// let it travel to the command, which turns it into a result the stationeer
/// can read.
/// </summary>
public sealed class ThingNotFound : Exception
{
    /// <param name="kind">What was sought, as the stationeer names it: "Computer" or "Housing".</param>
    /// <param name="thingId">The thing id handed in.</param>
    /// <param name="where">Where it was sought, such as " on this computer's network", or null.</param>
    /// <param name="available">The things it could have been, already joined, or null to leave them out.</param>
    public ThingNotFound(string kind, long thingId, string? where = null, string? available = null)
        : base(BuildMessage(kind, thingId, where, available))
    {
    }

    private static string BuildMessage(string kind, long thingId, string? where, string? available)
    {
        var message = $"{kind} {thingId} not found{where}.";
        return available == null ? message : message + $" Available: {available}";
    }
}
