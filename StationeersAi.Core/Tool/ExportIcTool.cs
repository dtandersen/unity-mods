using System;
using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: burn a computer's editor text onto a housing chip.</summary>
public sealed class ExportIcTool : ITool
{
    private readonly ExportChipCommand _command;

    public ExportIcTool(ExportChipCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "export_ic",
        "Copy a computer's editor text onto a housing chip and verify the burn by code hash. Takes the computer and the housing, each by thing id. Only ask for this when the stationeer agrees.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop holding the code, by thing id. Read it from search_structures."),
            new ToolParameter("device", "The housing to burn, by thing id. Read it from search_structures."),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var computer = arguments.Get("computer");
        if (string.IsNullOrWhiteSpace(computer))
        {
            return new ToolFailure("Missing required argument 'computer'.");
        }

        long computerId;
        if (!ThingId.TryParse(computer, out computerId))
        {
            return new ToolFailure(ThingId.NotAThingId("computer", computer));
        }

        var device = arguments.Get("device");
        if (string.IsNullOrWhiteSpace(device))
        {
            return new ToolFailure("Missing required argument 'device'.");
        }

        long deviceId;
        if (!ThingId.TryParse(device, out deviceId))
        {
            return new ToolFailure(ThingId.NotAThingId("device", device));
        }

        ComputerActionResult result;
        try
        {
            result = _command.Execute(new ExportChipRequest(computerId, deviceId));
        }
        catch (Exception)
        {
            return new ToolFailure("Export failed.");
        }

        if (result.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(result, "Export failed."));
        }

        return new ToolSuccess<ComputerActionResult>(
            result,
            $"Exported {result.Lines} {(result.Lines == 1 ? "line" : "lines")} to {result.Device}. Chip hash matches the editor.");
    }
}
