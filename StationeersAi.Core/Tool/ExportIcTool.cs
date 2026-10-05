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
        "Copy a computer's editor text onto a housing chip and verify the burn by code hash. Takes the computer and the housing, each by label or thing id. Only ask for this when the stationeer agrees.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop holding the code, by label or thing id."),
            new ToolParameter("device", "The housing to burn, by label or thing id."),
            new ToolParameter("import", "Pull the housing chip into the editor instead of burning. Defaults to false.", false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var computer = arguments.Get("computer");
        if (string.IsNullOrWhiteSpace(computer))
        {
            return new ToolFailure("Missing required argument 'computer'.");
        }

        var device = arguments.Get("device");
        if (string.IsNullOrWhiteSpace(device))
        {
            return new ToolFailure("Missing required argument 'device'.");
        }

        var import = arguments.Get("import");
        var result = _command.Execute(computer, device, null, !Truthy(import), Truthy(import));
        if (result == null || !result.Written)
        {
            return new ToolFailure(result?.Error ?? "Export failed.");
        }

        if (Truthy(import))
        {
            return new ToolSuccess<IcWriteResult>(
                result,
                $"Pulled {result.Lines} {(result.Lines == 1 ? "line" : "lines")} from {device} into {result.Device}.");
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
