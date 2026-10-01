using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Tool: find live logic devices by name, optionally on one network.</summary>
    public sealed class SearchDevicesTool : ITool
    {
        private readonly SearchDevicesCommand _command;

        public SearchDevicesTool(SearchDevicesCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_devices",
            "Search live logic devices by name: labels, prefab types, reference ids, and data networks. Returns a Markdown table with columns Name, Type, ReferenceId, Network.",
            new[]
            {
                new ToolParameter("query", "Text to match against device names, e.g. 'vent'. Omit to list all.", Required: false),
                new ToolParameter("computer", "Limit to one computer's data network, by label or reference id. Omit for the whole world.", Required: false),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var devices = _command.Execute(arguments.Get("query"), arguments.Get("computer"))
                .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .ToArray();

            return new ToolSuccess<SearchDevicesResult>(
                new SearchDevicesResult(devices),
                FormatContent(arguments.Get("query"), devices));
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
                device.ReferenceId.ToString(),
                device.NetworkId == 0 ? "none" : device.NetworkId.ToString(),
            });

            return Markdown.Build(new[] { "Name", "Type", "ReferenceId", "Network" }, rows);
        }
    }
}
