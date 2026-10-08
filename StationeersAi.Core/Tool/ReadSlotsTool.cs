using System;
using System.Linq;
using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: list what one thing holds, slot by slot.</summary>
public sealed class ReadSlotsTool : ITool
{
    private readonly ReadThingSlotsCommand _command;

    public ReadSlotsTool(ReadThingSlotsCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "read_slots",
        "List the slots of one thing and what sits in each: a belt, a crate, a held tool. Takes the thing's label or reference id. Reads only.",
        new[]
        {
            new ToolParameter("thing", "The thing to look inside, by label or reference id. Read it from search_things or search_devices."),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var thing = arguments.Get("thing");
        if (string.IsNullOrWhiteSpace(thing))
        {
            return new ToolFailure("Missing required argument 'thing'.");
        }

        SlotsResult answer;
        try
        {
            answer = _command.Execute(new ReadThingSlotsRequest(thing));
        }
        catch (Exception)
        {
            return new ToolFailure("Thing slots read failed.");
        }

        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Thing not found."));
        }

        var slots = answer.Slots!;
        if (slots.Count == 0)
        {
            return new ToolSuccess<SlotsResult>(
                answer,
                $"Thing '{thing}' has no slots to read.");
        }

        var rows = slots.Select(slot => new[]
        {
            slot.Slot,
            slot.Name,
            slot.Quantity ?? string.Empty,
        });

        return new ToolSuccess<SlotsResult>(
            answer,
            Markdown.Build(new[] { "Slot", "Name", "Quantity" }, rows));
    }
}
