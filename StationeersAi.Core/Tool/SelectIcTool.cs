using System;
using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: point a computer's dropdown at one housing.</summary>
public sealed class SelectIcTool : ITool
{
    private readonly SelectHousingCommand _command;

    public SelectIcTool(SelectHousingCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "select_ic",
        "Point a computer's dropdown at one housing on its network. Moves nothing but the selection: write_ic guards on the hash of whatever it points at now.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop, by thing id. Read it from search_devices."),
            new ToolParameter("device", "The housing to point at, by thing id. Read it from search_devices."),
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
            result = _command.Execute(new SelectHousingRequest(computerId, deviceId));
        }
        catch (Exception)
        {
            return new ToolFailure("Select failed.");
        }

        if (result.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(result, "Select failed."));
        }

        return new ToolSuccess<ComputerActionResult>(
            result,
            $"The dropdown now points at {result.Device}.");
    }
}
