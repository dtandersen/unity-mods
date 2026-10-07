using System;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: export a computer's editor text to a housing chip.</summary>
public sealed class ExportIcTool : ITool
{
    private readonly UseComputerCommand _command;

    public ExportIcTool(UseComputerCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "export_ic",
        "Copy a computer's editor text onto a housing chip and verify the burn by code hash. Takes the computer and the housing, each by thing id. Only ask for this when the stationeer agrees.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop holding the code, by thing id. Read it from search_devices."),
            new ToolParameter("device", "The housing to burn, by thing id. Read it from search_devices."),
            new ToolParameter("import", "Pull the housing chip into the editor instead of burning. Defaults to false.", false),
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

        var import = arguments.Get("import");
        var result = _command.Execute(computerId, deviceId, null, null, !Truthy(import), Truthy(import));
        if (result == null || !result.Written)
        {
            return new ToolFailure(result?.Error ?? "Export failed.");
        }

        if (Truthy(import))
        {
            return new ToolSuccess<IcWriteResult>(
                result,
                $"Pulled {result.Lines} {(result.Lines == 1 ? "line" : "lines")} from housing {deviceId} into {result.Device}.");
        }

        return new ToolSuccess<IcWriteResult>(
            result,
            $"Exported {result.Lines} {(result.Lines == 1 ? "line" : "lines")} to {result.Device}. Chip hash matches the editor.");
    }

    private static bool Truthy(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Equals("0", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("false", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("no", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("off", StringComparison.OrdinalIgnoreCase);
    }
}
