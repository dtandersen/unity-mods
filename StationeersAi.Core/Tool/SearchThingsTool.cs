using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: search loose world things (ores, ingots, plants, dropped items) near a point.</summary>
public sealed class SearchThingsTool : ITool
{
    private readonly SearchThingsCommand _command;

    public SearchThingsTool(SearchThingsCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_things",
        "Search loose world things by name: ores, ingots, plants, dropped items, wreckage. A query filters by name; origin is 'x, y, z' in meters (omit it for your position); distance is a radius in meters (omit it for everywhere). Nearest first, 10 at a time. Returns a Markdown table with columns Name, Type, Thing, Quantity, Distance, Direction, Room. Direction is the bearing in degrees relative to where you face (0 ahead, positive to the right); 'here' means at your feet.",
        new[]
        {
            new ToolParameter("query", "Name fragment, e.g. 'ore'. Omit to list everything in range.", Required: false),
            new ToolParameter("origin", "Origin as 'x, y, z', e.g. '0, 0, 0'. Omit for your position.", Required: false),
            new ToolParameter("distance", "Radius in meters, e.g. '10'. Omit for everywhere.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var origin = ParseOrigin(arguments.Get("origin"));
        if (!origin.Parsed)
        {
            return new ToolFailure($"Origin '{arguments.Get("origin")}' is not 'x, y, z'.");
        }

        var distance = ParseDistance(arguments.Get("distance"));
        if (!distance.Parsed)
        {
            return new ToolFailure(distance.Error);
        }

        var search = _command.Search(arguments.Get("query"), origin.Value, distance.Value);
        var total = search.Things.Count;
        if (total == 0)
        {
            return new ToolSuccess<SearchThingsResult>(
                new SearchThingsResult(search.Things),
                string.IsNullOrWhiteSpace(arguments.Get("query"))
                    ? "No things found."
                    : $"No things found for '{arguments.Get("query")}'.");
        }

        var page = search.Things
            .Select(thing => new { Thing = thing, Distance = Distance(thing, search.Origin) })
            .OrderBy(match => match.Distance ?? double.MaxValue)
            .ThenBy(match => match.Thing.Name, StringComparer.OrdinalIgnoreCase)
            .Take(ToolLimits.MaxResults)
            .ToArray();

        var rows = page.Select(match => new[]
        {
            match.Thing.Name ?? string.Empty,
            match.Thing.Type ?? string.Empty,
            match.Thing.Thing.ToString(),
            match.Thing.Quantity ?? "1",
            match.Distance.HasValue ? $"{match.Distance.Value:0.#} m" : "?",
            Bearing(match.Thing, search.Origin, search.Facing),
            match.Thing.RoomId == 0 ? "none" : match.Thing.RoomId.ToString(),
        });

        return new ToolSuccess<SearchThingsResult>(
            new SearchThingsResult(search.Things),
            Markdown.Build(new[] { "Name", "Type", "Thing", "Quantity", "Distance", "Direction", "Room" }, rows) +
            $"\nShowing {page.Length} of {total}.");
    }

    private static string Bearing(ThingInfo thing, OriginPoint origin, double? facing)
    {
        if (origin == null || !facing.HasValue)
        {
            return "?";
        }

        var dx = thing.X - origin.X;
        var dz = thing.Z - origin.Z;
        if (dx == 0 && dz == 0)
        {
            return "here";
        }

        var relative = Math.Atan2(dx, -dz) * 180.0 / Math.PI - facing.Value;
        while (relative > 180.0)
        {
            relative -= 360.0;
        }

        while (relative <= -180.0)
        {
            relative += 360.0;
        }

        var rounded = (int)Math.Round(relative);
        return rounded == 0 ? "0" : (rounded > 0 ? "+" : string.Empty) + rounded;
    }

    private static double? Distance(ThingInfo thing, OriginPoint origin)
    {
        if (origin == null)
        {
            return null;
        }

        var dx = thing.X - origin.X;
        var dy = thing.Y - origin.Y;
        var dz = thing.Z - origin.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static (bool Parsed, OriginPoint Value) ParseOrigin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (true, null);
        }

        var parts = value.Split(',');
        if (parts.Length != 3)
        {
            return (false, null);
        }

        var numbers = new double[3];
        for (var i = 0; i < 3; i++)
        {
            if (!double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i]))
            {
                return (false, null);
            }
        }

        return (true, new OriginPoint(numbers[0], numbers[1], numbers[2]));
    }

    private static (bool Parsed, double? Value, string Error) ParseDistance(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (true, null, null);
        }

        double distance;
        if (!double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out distance))
        {
            return (false, null, $"Distance '{value}' is not a number.");
        }

        if (distance < 0)
        {
            return (false, null, $"Distance '{value}' must be zero or more.");
        }

        return (true, distance, null);
    }
}
