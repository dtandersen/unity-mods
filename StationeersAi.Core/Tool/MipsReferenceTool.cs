using StationeersAi.Command;
using System.Linq;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: look up IC10 instructions from the game's editor help.</summary>
public sealed class MipsReferenceTool : ITool
{
    private readonly ReadInstructionsCommand _command;

    public MipsReferenceTool(ReadInstructionsCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "mips_reference",
        "Look up IC10 instructions: usage and what each one does. Name one for its usage and description; omit the argument for the bare name list.",
        new[] { new ToolParameter("instruction", "One instruction mnemonic, e.g. 'l'.", Required: false) });

    public ToolResult Execute(ToolArguments arguments)
    {
        var instruction = arguments.Get("instruction");
        var instructions = _command.Execute(instruction);
        if (instructions.Count == 0)
        {
            return new ToolFailure($"Unknown MIPS instruction '{instruction}'.");
        }

        if (string.IsNullOrWhiteSpace(instruction))
        {
            return new ToolSuccess<MipsReferenceResult>(
                new MipsReferenceResult(instructions),
                string.Join("\n", instructions.Select(entry => entry.Name)));
        }

        var lines = new System.Collections.Generic.List<string>();
        foreach (var entry in instructions)
        {
            var line = $"{entry.Name}: {entry.Example}";
            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                line += $" - {entry.Description}";
            }

            lines.Add(line);
        }

        return new ToolSuccess<MipsReferenceResult>(
            new MipsReferenceResult(instructions),
            string.Join("\n", lines));
    }
}
