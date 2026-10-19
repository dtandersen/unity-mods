using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: find built structures by name, optionally logic ones on one cable network.</summary>
public sealed class SearchStructuresTool : ITool
{
    private readonly SearchStructuresCommand _command;

    public SearchStructuresTool(SearchStructuresCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_structures",
        "Search built structures by name: structures, pipes, walls. Labels, prefab names, ids, wire lists, rooms, positions in meters, and whether each speaks logic. Logic structures list first, 10 at a time (30 for logic-only); pass offset to page. Returns a Markdown table with columns Name, PrefabName, Id, Connections, Logic, Room, Position.",
        new[]
        {
            new ToolParameter("query", "Text to match against structure names, e.g. 'vent'. Omit to list all.", Required: false),
            new ToolParameter("powerNetworkId", "Limit to one power network id, e.g. '29457'. Omit for every power network.", Required: false),
            new ToolParameter("dataNetworkId", "Limit to one data network id, e.g. '7'. Omit for every data network.", Required: false),
            new ToolParameter("logic", "Only logic structures ('true') or only dumb objects ('false'). Omit for both.", Required: false),
            new ToolParameter("offset", "Skip this many matches before listing. Omit to start at the top.", Required: false),
            new ToolParameter("room", "Limit to one room id, e.g. '5'. Omit for everywhere.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var powerRaw = arguments.Get("powerNetworkId") ?? arguments.Get("cableNetworkId");
        var powerNetworkId = ParseLong(powerRaw);
        if (!powerNetworkId.Parsed)
        {
            return new ToolFailure($"PowerNetworkId '{powerRaw}' is not a number.");
        }

        var dataNetworkId = ParseLong(arguments.Get("dataNetworkId"));
        if (!dataNetworkId.Parsed)
        {
            return new ToolFailure($"DataNetworkId '{arguments.Get("dataNetworkId")}' is not a number.");
        }

        var room = ParseLong(arguments.Get("room"));
        if (!room.Parsed)
        {
            return new ToolFailure($"Room '{arguments.Get("room")}' is not a number.");
        }

        var logic = ParseLogic(arguments.Get("logic"));
        if (!logic.Parsed)
        {
            return new ToolFailure($"Logic '{arguments.Get("logic")}' is not true or false.");
        }

        var offset = ParseOffset(arguments.Get("offset"));
        if (!offset.Parsed)
        {
            return new ToolFailure($"Offset '{arguments.Get("offset")}' is not a number.");
        }

        var matches = _command.Execute(new SearchStructuresRequest(arguments.Get("query"), powerNetworkId.Value, dataNetworkId.Value, logic.Value, room.Value)).Structures
            .OrderByDescending(structure => structure.Logic)
            .ThenBy(structure => structure.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var cap = logic.Value == true ? ToolLimits.MaxLogicResults : ToolLimits.MaxResults;
        var page = matches.Skip(offset.Value).Take(cap).ToArray();

        return new ToolSuccess<SearchStructuresResult>(
            new SearchStructuresResult(page),
            FormatContent(arguments.Get("query"), matches.Length, page));
    }

    private static (bool Parsed, bool? Value) ParseLogic(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return (true, null);
        }

        var text = value.Trim();
        if (text.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return (true, true);
        }

        if (text.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return (true, false);
        }

        return (false, null);
    }

    private static (bool Parsed, int Value) ParseOffset(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return (true, 0);
        }

        int offset;
        if (int.TryParse(value.Trim(), out offset))
        {
            return (true, Math.Max(0, offset));
        }

        return (false, 0);
    }

    private static (bool Parsed, long? Value) ParseLong(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return (true, null);
        }

        long number;
        if (long.TryParse(value.Trim(), out number))
        {
            return (true, number);
        }

        return (false, null);
    }

    private static string FormatContent(string? query, int total, IReadOnlyList<StructureInfo> structures)
    {
        if (total == 0)
        {
            return string.IsNullOrWhiteSpace(query)
                ? "No structures found."
                : $"No structures found for '{query}'.";
        }

        var rows = structures.Select(structure => new[]
        {
            structure.Name ?? string.Empty,
            structure.PrefabName ?? string.Empty,
            structure.Id.ToString(),
            NetworkConnections.Format(structure.Connections),
            structure.Logic ? "yes" : "no",
            structure.RoomId == 0 ? "none" : structure.RoomId.ToString(),
            $"{structure.Position.X:0.0}, {structure.Position.Y:0.0}, {structure.Position.Z:0.0}",
        });

        return Markdown.Build(new[] { "Name", "PrefabName", "Id", "Connections", "Logic", "Room", "Position" }, rows) +
            $"\nShowing {structures.Count} of {total}.";
    }
}
