using System;

namespace Sage.Tool;

/// <summary>
/// Reads a thing id from a tool argument. Devices are addressed by id, so a
/// label reaching a tool is a mistake the model can fix once told.
/// </summary>
internal static class ThingId
{
    /// <summary>Parses <paramref name="value"/>; false when it is not a thing id.</summary>
    internal static bool TryParse(string? value, out long id)
    {
        id = 0;
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return long.TryParse(value.Trim(), out id);
    }

    /// <summary>The sentence naming the argument that failed to parse.</summary>
    internal static string NotAThingId(string argument, string? value)
    {
        return $"'{argument}' is not a thing id: '{value}'. Read it from search_structures.";
    }
}
