using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Formatting;

namespace StationeersAi.Tool
{
    /// <summary>Tool: read a computer's editor text, e.g. to review before overwriting.</summary>
    public sealed class ReadIcTool : ITool
    {
        private readonly ReadIcCommand _command;

        public ReadIcTool(ReadIcCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "read_ic",
            "Read the MIPS code of an IC editor.",
            new[]
            {
                new ToolParameter("computer_name", "The computer or laptop, by label.", Required: false),
                new ToolParameter("computer_thing", "The computer or laptop, by thing id. Wins over the label when both are given.", Required: false),
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

            var result = _command.Execute(name, thing.Value);
            if (result == null || !result.Found)
            {
                return new ToolFailure(result?.Error ?? "Computer not found.");
            }

            return new ToolSuccess<IcReadResult>(
                result,
                string.IsNullOrEmpty(result.Code)
                    ? "(editor is empty)"
                    : result.Code + $"\nCode hash: {CodeHash.Hash(result.Code)}");
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
