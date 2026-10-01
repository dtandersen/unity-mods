using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
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
            "Describe one live device in full: its logic settings with live values, and its slots with occupants. Find devices first with search_devices.",
            new[] { new ToolParameter("device", "The device, by label or reference id.") });

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
                return new ToolFailure($"No single device matches '{device}'.");
            }

            return new ToolSuccess<DescribeDeviceResult>(
                new DescribeDeviceResult(details),
                Describe(details));
        }

        private static string Describe(DeviceDetails details)
        {
            var lines = new List<string>
            {
                $"Device: {details.Name ?? "unknown"} ({details.Type ?? "unknown"} #{details.Thing})",
                $"Network: {(details.NetworkId == 0 ? "none" : details.NetworkId.ToString())}" +
                    (details.IsOn == null ? string.Empty : $", On: {(details.IsOn.Value ? "yes" : "no")}"),
            };

            var logic = details.Logic ?? Array.Empty<DeviceLogicValue>();
            if (logic.Count == 0)
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

            return string.Join("\n", lines);
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
}
