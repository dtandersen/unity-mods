using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: find built things by name, optionally logic ones on one network.</summary>
public sealed class SearchDevicesTool : ITool
{
    private readonly SearchDevicesCommand _command;

    public SearchDevicesTool(SearchDevicesCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_devices",
        "Search built things by name: devices, pipes, walls. Labels, prefab types, thing ids, data networks, rooms, and whether each speaks logic. Logic devices list first, 10 at a time (30 for logic-only); pass offset to page. Returns a Markdown table with columns Name, Type, Thing, Network, Logic, Room.",
        new[]
        {
            new ToolParameter("query", "Text to match against device names, e.g. 'vent'. Omit to list all.", Required: false),
            new ToolParameter("network", "Limit to one data network id, e.g. '7'. Omit for every network.", Required: false),
            new ToolParameter("logic", "Only logic devices ('true') or only dumb objects ('false'). Omit for both.", Required: false),
            new ToolParameter("offset", "Skip this many matches before listing. Omit to start at the top.", Required: false),
            new ToolParameter("room", "Limit to one room id, e.g. '5'. Omit for everywhere.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var network = ParseLong(arguments.Get("network"));
        if (!network.Parsed)
        {
            return new ToolFailure($"Network '{arguments.Get("network")}' is not a number.");
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

        var matches = _command.Execute(arguments.Get("query"), network.Value, logic.Value, room.Value)
            .OrderByDescending(device => device.Logic)
            .ThenBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var cap = logic.Value == true ? ToolLimits.MaxLogicResults : ToolLimits.MaxResults;
        var page = matches.Skip(offset.Value).Take(cap).ToArray();

        return new ToolSuccess<SearchDevicesResult>(
            new SearchDevicesResult(page),
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

    private static string FormatContent(string? query, int total, IReadOnlyList<DeviceInfo> devices)
    {
        if (total == 0)
        {
            return string.IsNullOrWhiteSpace(query)
                ? "No devices found."
                : $"No devices found for '{query}'.";
        }

        var rows = devices.Select(device => new[]
        {
            device.Name ?? string.Empty,
            device.Type ?? string.Empty,
            device.Thing.ToString(),
            device.NetworkId == 0 ? "none" : device.NetworkId.ToString(),
            device.Logic ? "yes" : "no",
            device.RoomId == 0 ? "none" : device.RoomId.ToString(),
        });

        return Markdown.Build(new[] { "Name", "Type", "Thing", "Network", "Logic", "Room" }, rows) +
            $"\nShowing {devices.Count} of {total}.";
    }
}
