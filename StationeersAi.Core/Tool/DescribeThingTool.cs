using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: describe one live thing in full, structure or loose.</summary>
public sealed class DescribeThingTool : ITool
{
    private readonly DescribeThingCommand _command;

    public DescribeThingTool(DescribeThingCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "describe_thing",
        "Describe one live thing: a built structure or a loose item. A structure match returns its logic settings with live values, slots, build progress, and power/data networks; a loose match returns its quantity and position. Find it first with search_things or search_structures.",
        new[]
        {
            new ToolParameter("thing", "The thing, by label or reference id."),
            new ToolParameter("full", "Show every logic setting including zeros ('true'). Omit for the short list.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var thing = arguments.Get("thing");
        if (string.IsNullOrWhiteSpace(thing))
        {
            return new ToolFailure("Missing required argument 'thing'.");
        }

        var answer = _command.Execute(new DescribeThingRequest(thing));
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Thing not found.") + Matches(answer));
        }

        var full = IsTrue(arguments.Get("full"));
        return new ToolSuccess<DescribeThingResult>(
            new DescribeThingResult(answer.Details!),
            Describe(answer.Details!, full));
    }

    private static bool IsTrue(string? value)
    {
        return value is not null && !string.IsNullOrWhiteSpace(value) &&
            (value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    private static string Matches(ThingDescribeResult answer)
    {
        var parts = new List<string>();
        if (answer.StructureCandidates.Count > 0)
        {
            parts.Add(
                " Structures:" +
                "\n" + Markdown.Build(
                    new[] { "Name", "PrefabName", "Id", "Connections" },
                    answer.StructureCandidates.Select(entry => new[]
                    {
                        entry.Name ?? string.Empty,
                        entry.PrefabName ?? string.Empty,
                        entry.Id.ToString(),
                        NetworkConnections.Format(entry.Connections),
                    })));
        }

        if (answer.ThingCandidates.Count > 0)
        {
            parts.Add(
                " Things:" +
                "\n" + Markdown.Build(
                    new[] { "Name", "PrefabName", "Id", "Quantity" },
                    answer.ThingCandidates.Select(entry => new[]
                    {
                        entry.Name ?? string.Empty,
                        entry.PrefabName ?? string.Empty,
                        entry.Id.ToString(),
                        entry.Quantity ?? "1",
                    })));
        }

        return parts.Count == 0 ? string.Empty : string.Join(string.Empty, parts);
    }

    /// <summary>Most useful first: switch, power, air, then the rest by name.</summary>
    private static readonly string[] Priority = new[] { "On", "Power", "Temperature", "Pressure", "Setting", "Mode", "Error" };

    private static string DescribeBuild(BuildProgress? build)
    {
        if (build == null)
        {
            return "Build states: unknown.";
        }

        if (build.Stage >= build.Stages)
        {
            return "Build states: complete.";
        }

        var lines = new List<string> { $"Build states: {build.Stage}/{build.Stages}." };
        foreach (var step in build.BuildStates ?? Array.Empty<BuildStep>())
        {
            lines.Add(BuildStepText.Format(step));
        }

        return string.Join("\n", lines);
    }

    internal static string Describe(ThingDetails thing, bool full)
    {
        var lines = new List<string>
        {
            $"Thing: {thing.Name ?? "unknown"} ({thing.PrefabName ?? "unknown"} #{thing.Id}) [{thing.Kind}]",
            $"Quantity: {thing.Quantity ?? "none"}",
            $"Position: {thing.Position.X:0.0}, {thing.Position.Y:0.0}, {thing.Position.Z:0.0}, Room: {(thing.RoomId == 0 ? "none" : thing.RoomId.ToString())}",
            $"Connections: {NetworkConnections.Format(thing.Connections)}, Logic: {(thing.Logic ? "yes" : "no")}",
        };

        var details = thing.Details;
        if (details == null)
        {
            lines.Add("Structure details: none (loose thing).");
            return string.Join("\n", lines);
        }

        lines.Add(
            $"On: {(details.IsOn == null ? "none" : (details.IsOn.Value ? "yes" : "no"))}");
        lines.Add(DescribeBuild(details.Build));

        var logic = (details.Logic ?? Array.Empty<StructureLogicValue>())
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

        var slots = details.Slots ?? Array.Empty<StructureSlotInfo>();
        if (slots.Count == 0)
        {
            lines.Add("Slots: none.");
        }
        else
        {
            lines.Add("Slots:");
            lines.Add(Markdown.Build(
                new[] { "#", "SlotType", "Occupant" },
                slots.Select(slot => new[] { slot.Index.ToString(), slot.SlotType, slot.Occupant })));
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
            lines.Add("Structures:");
            lines.Add(Markdown.Build(
                new[] { "Index", "Label", "Id" },
                details.Devices.Select(entry => new[]
                {
                    entry.Index,
                    entry.Label,
                    entry.Id == 0 ? "disconnected" : $"{entry.Label} #{entry.Id}",
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
