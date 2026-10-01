using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Tool: write MIPS source to a computer's editor. Never burns chips.</summary>
    public sealed class WriteIcTool : ITool
    {
        /// <summary>The editor's line cap (line indices 0-127).</summary>
        public const int MaxCodeLines = 127;

        private readonly WriteIcCommand _command;

        public WriteIcTool(WriteIcCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "write_ic",
            "Place MIPS source in a computer or laptop's IC editor so the stationeer can review and Export it. Takes the computer's label ('computer_name') or thing id ('computer_thing') and the full code. Never writes to housings or chips.",
            new[]
            {
                new ToolParameter("computer_name", "The computer or laptop, by label.", Required: false),
                new ToolParameter("computer_thing", "The computer or laptop, by thing id. Wins over the label when both are given.", Required: false),
                new ToolParameter("code", "The full MIPS source for its editor, 127 lines max."),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var name = arguments.Get("computer_name");
            var thing = ParseThing(arguments.Get("computer_thing"));
            if (!thing.Parsed)
            {
                return new ToolFailure($"Computer thing '{arguments.Get("computer_thing")}' is not a number.");
            }

            if (string.IsNullOrWhiteSpace(name) && !thing.Value.HasValue)
            {
                return new ToolFailure("Give the computer's label ('computer_name') or thing id ('computer_thing').");
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

            var result = _command.Execute(name, thing.Value, code);
            if (result == null || !result.Written)
            {
                return new ToolFailure(result?.Error ?? "Computer not found.");
            }

            return new ToolSuccess<IcWriteResult>(
                result,
                $"Wrote {result.Lines} {(result.Lines == 1 ? "line" : "lines")} to {result.Device}. The stationeer can review it in the editor and press Export.");
        }

        private static (bool Parsed, long? Value) ParseThing(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (true, null);
            }

            long thing;
            if (long.TryParse(value.Trim(), out thing))
            {
                return (true, thing);
            }

            return (false, null);
        }
    }
}
