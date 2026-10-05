using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: report the player's current status (position, world, condition).</summary>
public sealed class GetStatusTool : ITool
{
    private readonly GetPlayerCommand _command;

    public GetStatusTool(GetPlayerCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "get_status",
        "Report the player's current status: position, world, and condition summary. Takes no arguments.",
        new ToolParameter[0]);

    public ToolResult Execute(ToolArguments arguments)
    {
        var status = _command.Execute();
        if (status == null)
        {
            return new ToolFailure("Player status is unavailable.");
        }

        var content = $"Position: ({status.X:0.0}, {status.Y:0.0}, {status.Z:0.0})\nWorld: {status.World ?? "unknown"}\nRoom: {(status.RoomId.HasValue ? status.RoomId.Value.ToString() : "outside")}";
        if (!string.IsNullOrWhiteSpace(status.Details))
        {
            content += $"\n{status.Details}";
        }

        return new ToolSuccess<GetStatusResult>(
            new GetStatusResult(status),
            content);
    }
}
