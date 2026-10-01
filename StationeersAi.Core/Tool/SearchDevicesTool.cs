using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
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
            "Search built things by name: devices, pipes, walls. Labels, prefab types, thing ids, data networks, and whether each speaks logic. Returns a Markdown table with columns Name, Type, Thing, Network, Logic.",
            new[]
            {
                new ToolParameter("query", "Text to match against device names, e.g. 'vent'. Omit to list all.", Required: false),
                new ToolParameter("network", "Limit to one data network id, e.g. '7'. Omit for every network.", Required: false),
                new ToolParameter("logic", "Only logic devices ('true') or only dumb objects ('false'). Omit for both.", Required: false),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var network = ParseNetwork(arguments.Get("network"));
            if (!network.Parsed)
            {
                return new ToolFailure($"Network '{arguments.Get("network")}' is not a number.");
            }

            var logic = ParseLogic(arguments.Get("logic"));
            if (!logic.Parsed)
            {
                return new ToolFailure($"Logic '{arguments.Get("logic")}' is not true or false.");
            }

            var devices = _command.Execute(arguments.Get("query"), network.Value, logic.Value)
                .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .ToArray();

            return new ToolSuccess<SearchDevicesResult>(
                new SearchDevicesResult(devices),
                FormatContent(arguments.Get("query"), devices));
        }

        private static (bool Parsed, bool? Value) ParseLogic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
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

        private static (bool Parsed, long? Value) ParseNetwork(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (true, null);
            }

            long network;
            if (long.TryParse(value.Trim(), out network))
            {
                return (true, network);
            }

            return (false, null);
        }

        private static string FormatContent(string query, IReadOnlyList<DeviceInfo> devices)
        {
            if (devices.Count == 0)
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
            });

            return Markdown.Build(new[] { "Name", "Type", "Thing", "Network", "Logic" }, rows);
        }
    }
}
