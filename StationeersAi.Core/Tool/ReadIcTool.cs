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
            "Read the full text in a computer or laptop's IC editor. Takes the computer, by label or thing id.",
            new[]
            {
                new ToolParameter("computer", "The computer or laptop, by label or thing id."),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var computer = arguments.Get("computer");
            if (string.IsNullOrWhiteSpace(computer))
            {
                return new ToolFailure("Missing required argument 'computer'.");
            }

            var result = _command.Execute(computer);
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

    }
}
