using System;
using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: pull a housing chip into a computer's editor.</summary>
public sealed class ImportIcTool : ITool
{
    private readonly ImportChipCommand _command;

    public ImportIcTool(ImportChipCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "import_ic",
        "Pull a housing chip's burned program into a computer's editor, overwriting the screen text. Takes the housing and the computer, each by thing id. Burns nothing; read_ic shows what landed on the screen.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop whose screen takes the text, by thing id. Read it from search_devices."),
            new ToolParameter("device", "The housing to pull from, by thing id. Read it from search_devices."),
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
            result = _command.Execute(new ImportChipRequest(computerId, deviceId));
        }
        catch (Exception)
        {
            return new ToolFailure("Import failed.");
        }

        if (result.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(result, "Import failed."));
        }

        return new ToolSuccess<ComputerActionResult>(
            result,
            $"Pulled {result.Lines} {(result.Lines == 1 ? "line" : "lines")} from housing {deviceId} into {result.Device}.");
    }
}
