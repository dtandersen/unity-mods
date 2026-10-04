using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: describe one live device in full: ports, settings, slots.</summary>
public sealed class DescribeDeviceTool : ITool
{
    private readonly DescribeDeviceCommand _command;

    public DescribeDeviceTool(DescribeDeviceCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "describe_device",
        "Describe one live device: its logic settings with live values, and its slots with occupants. Find devices first with search_devices.",
        new[]
        {
            new ToolParameter("device", "The device, by label or reference id."),
            new ToolParameter("full", "Show every logic setting including zeros ('true'). Omit for the short list.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var device = arguments.Get("device");
        if (string.IsNullOrWhiteSpace(device))
        {
            return new ToolFailure("Missing required argument 'device'.");
        }

        var details = _command.Execute(device);
        if (details == null)
        {
            var candidates = _command.Candidates(device);
            if (candidates.Count == 0)
            {
                return new ToolFailure($"No single device matches '{device}'.");
            }

            return new ToolFailure(
                $"No single device matches '{device}'. Matches:" +
                "\n" + Markdown.Build(
                    new[] { "Name", "Type", "Thing", "Network" },
                    candidates.Select(entry => new[]
                    {
                        entry.Name ?? string.Empty,
                        entry.Type ?? string.Empty,
                        entry.Thing.ToString(),
                        entry.NetworkId == 0 ? "none" : entry.NetworkId.ToString(),
                    })));
        }

        var full = IsTrue(arguments.Get("full"));
        return new ToolSuccess<DescribeDeviceResult>(
            new DescribeDeviceResult(details),
            Describe(details, full));
    }

    private static bool IsTrue(string? value)
    {
        return value is not null && !string.IsNullOrWhiteSpace(value) &&
            (value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Most useful first: switch, power, air, then the rest by name.</summary>
    private static readonly string[] Priority = new[] { "On", "Power", "Temperature", "Pressure", "Setting", "Mode", "Error" };

    private static string Describe(DeviceDetails details, bool full)
    {
        var lines = new List<string>
        {
            $"Device: {details.Name ?? "unknown"} ({details.Type ?? "unknown"} #{details.Thing})",
            $"Network: {(details.NetworkId == 0 ? "none" : details.NetworkId.ToString())}" +
                (details.IsOn == null ? string.Empty : $", On: {(details.IsOn.Value ? "yes" : "no")}"),
        };

        var logic = (details.Logic ?? Array.Empty<DeviceLogicValue>())
            .Where(entry => full || entry.Value != 0)
            .OrderBy(entry => PriorityIndex(entry.Name))
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (logic.Length == 0)
        {
            lines.Add("Logic: none readable.");
        }
        else
        {
            lines.Add("Logic:");
            lines.Add(Markdown.Build(
                new[] { "Name", "Value", "Writable" },
                logic.Select(entry => new[] { entry.Name, Format(entry.Value), entry.Writable ? "yes" : "no" })));
        }

        var slots = details.Slots ?? Array.Empty<DeviceSlotInfo>();
        if (slots.Count == 0)
        {
            lines.Add("Slots: none.");
        }
        else
        {
            lines.Add("Slots:");
            lines.Add(Markdown.Build(
                new[] { "#", "Slot", "Occupant" },
                slots.Select(slot => new[] { slot.Index.ToString(), slot.Slot, slot.Occupant })));
        }

        if (details.Storage != null)
        {
            var stored = details.Storage;
            if (stored.Count == 0)
            {
                lines.Add("Storage: empty.");
            }
            else
            {
                lines.Add("Storage:");
                lines.Add(Markdown.Build(
                    new[] { "Reagent", "Quantity" },
                    stored.Select(entry => new[] { entry.Reagent, Format(entry.Quantity) })));
            }
        }

        if (details.Devices != null)
        {
            lines.Add("Devices:");
            lines.Add(Markdown.Build(
                new[] { "Index", "Label", "Thing" },
                details.Devices.Select(entry => new[]
                {
                    entry.Index,
                    entry.Label,
                    entry.Thing == 0 ? "disconnected" : $"{entry.Label} #{entry.Thing}",
                })));
        }

        if (details.Chip != null)
        {
            lines.Add("Chip:");
            lines.Add(details.Chip.ErrorType == null
                ? "Error: none"
                : $"Error: {details.Chip.ErrorType}" +
                    (details.Chip.ErrorLine == null ? string.Empty : $" at line {details.Chip.ErrorLine}"));
            if (details.Chip.Display.HasValue)
            {
                lines.Add($"Display: {Format(details.Chip.Display.Value)}");
            }

            if (!string.IsNullOrEmpty(details.Chip.CodeHash))
            {
                lines.Add($"Code hash: {details.Chip.CodeHash}");
            }
        }

        return string.Join("\n", lines);
    }

    private static int PriorityIndex(string name)
    {
        for (var i = 0; i < Priority.Length; i++)
        {
            if (Priority[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return Priority.Length;
    }

    private static string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return value.ToString();
        }

        if (Math.Abs(value) < 1e15 && Math.Abs(value - Math.Round(value)) < 1e-9)
        {
            return ((long)Math.Round(value)).ToString();
        }

        return value.ToString("G6");
    }
}
