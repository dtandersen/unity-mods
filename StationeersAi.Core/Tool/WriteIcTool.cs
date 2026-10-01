using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Tool: write MIPS source to a computer's editor. Never burns chips.</summary>
    public sealed class WriteIcTool : ITool
    {
        private readonly WriteIcCommand _command;

        public WriteIcTool(WriteIcCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "write_ic",
            "Place MIPS source in a computer or laptop's IC editor so the stationeer can review and Export it. Takes the device (name or reference id) and the full code. Never writes to housings or chips.",
            new[]
            {
                new ToolParameter("device", "The computer or laptop, by label or reference id."),
                new ToolParameter("code", "The full MIPS source for its editor."),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var device = arguments.Get("device");
            if (string.IsNullOrWhiteSpace(device))
            {
                return new ToolFailure("Missing required argument 'device'.");
            }

            var code = arguments.Get("code");
            if (code == null)
            {
                return new ToolFailure("Missing required argument 'code'.");
            }

            var result = _command.Execute(device, code);
            if (result == null || !result.Written)
            {
                return new ToolFailure(result?.Error ?? $"Computer '{device}' not found.");
            }

            return new ToolSuccess<IcWriteResult>(
                result,
                $"Wrote {result.Lines} lines to {result.Device}. The stationeer can review it in the editor and press Export.");
        }
    }
}
