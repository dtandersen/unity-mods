using Sage.Command;
using System.Linq;
using Caev.Tools;

namespace Sage.Tool;

/// <summary>Tool: list IC10 instructions from the game's editor help.</summary>
public sealed class ListInstructionsTool : ITool
{
    private readonly ListInstructionsCommand _command;

    public ListInstructionsTool(ListInstructionsCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "list_instructions",
        "List IC10 instructions: usage and what each one does. Name one for its usage and description; omit the argument for the bare name list.",
        new[] { new ToolParameter("instruction", "One instruction mnemonic, e.g. 'l'.", Required: false) });

    public ToolResult Execute(ToolArguments arguments)
    {
        var instruction = arguments.Get("instruction");
        var answer = _command.Execute(new ListInstructionsRequest(instruction));
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Unknown MIPS instruction."));
        }

        var instructions = answer.Instructions;

        if (string.IsNullOrWhiteSpace(instruction))
        {
            return new ToolSuccess<ListInstructionsResult>(
                new ListInstructionsResult(instructions),
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

        return new ToolSuccess<ListInstructionsResult>(
            new ListInstructionsResult(instructions),
            string.Join("\n", lines));
    }
}
