using StationeersAi.Command;

namespace StationeersAi.Tool
{
    /// <summary>Tool: look up IC10 instructions from the game's editor help.</summary>
    public sealed class MipsReferenceTool : ITool
    {
        private readonly ReadMipsCommand _command;

        public MipsReferenceTool(ReadMipsCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "mips_reference",
            "Look up IC10 instructions: usage and what each one does. Omit the argument to list every instruction; name one for its full description.",
            new[] { new ToolParameter("instruction", "One instruction mnemonic, e.g. 'l'.", Required: false) });

        public ToolResult Execute(ToolArguments arguments)
        {
            var instruction = arguments.Get("instruction");
            var instructions = _command.Execute(instruction);
            if (instructions.Count == 0)
            {
                return new ToolFailure($"Unknown MIPS instruction '{instruction}'.");
            }

            var lines = new System.Collections.Generic.List<string>();
            foreach (var entry in instructions)
            {
                lines.Add($"{entry.Name}: {entry.Example}");
                if (!string.IsNullOrWhiteSpace(instruction) && !string.IsNullOrWhiteSpace(entry.Description))
                {
                    lines.Add(entry.Description);
                }
            }

            return new ToolSuccess<MipsReferenceResult>(
                new MipsReferenceResult(instructions),
                string.Join("\n", lines));
        }
    }
}
