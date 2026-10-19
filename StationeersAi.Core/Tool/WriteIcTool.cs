using System;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: write MIPS source to a computer's editor. Never burns chips.</summary>
public sealed class WriteIcTool : ITool
{
    /// <summary>The editor's line cap (line indices 0-127).</summary>
    public const int MaxCodeLines = 127;

    private readonly WriteComputerCommand _command;

    public WriteIcTool(WriteComputerCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "write_ic",
        "Place MIPS source in a computer or laptop's IC editor so the stationeer can review and Export it. Takes the computer's thing id, the hash of the chip now under that computer's dropdown, and the full code. The write refuses when the hash is not the chip in front of the editor, so a stale or wrong selection cannot lose a program. Never writes to housings or chips.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop, by thing id. Read it from search_structures."),
            new ToolParameter("hash", "The code hash of the chip under the dropdown. Read it from read_ic."),
            new ToolParameter("code", "The full MIPS source for its editor, 127 lines max."),
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

        var chipHash = arguments.Get("hash");
        if (chipHash is null || string.IsNullOrWhiteSpace(chipHash))
        {
            return new ToolFailure("Missing required argument 'hash'. Read the chip with read_ic and pass its hash.");
        }

        var code = arguments.Get("code");
        if (code == null)
        {
            return new ToolFailure("Missing required argument 'code'.");
        }

        var lines = code.Split('\n').Length;
        if (lines > MaxCodeLines)
        {
            return new ToolFailure($"Code is {lines} lines; the editor holds {MaxCodeLines}.");
        }

        ComputerActionResult result;
        try
        {
            result = _command.Execute(new WriteComputerRequest(computerId, code, chipHash.Trim()));
        }
        catch (Exception)
        {
            return new ToolFailure("Computer write failed.");
        }

        if (result.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(result, "Computer not found."));
        }

        return new ToolSuccess<ComputerActionResult>(
            result,
            $"Wrote {result.Lines} {(result.Lines == 1 ? "line" : "lines")} to {result.Device}. The stationeer can review it in the editor and press Export.");
    }
}
