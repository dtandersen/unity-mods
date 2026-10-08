using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Ask for one mnemonic, or for the whole list when it is blank.</summary>
public sealed record ListInstructionsRequest(string? Instruction);

/// <summary>The instruction entries found, or why none were.</summary>
public sealed record InstructionsResult(
    Status Status, List<string> Errors, IReadOnlyList<Instruction> Instructions) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: all instructions when the ask is blank, else the one
/// matching the mnemonic; a named instruction that no entry matches raises
/// <see cref="Status.NOT_FOUND"/>.
/// </summary>
public sealed class ListInstructionsCommand : Command<ListInstructionsRequest, InstructionsResult>
{
    private readonly IMipsReference _reference;

    public ListInstructionsCommand(IMipsReference reference)
    {
        _reference = reference;
    }

    public InstructionsResult Execute(ListInstructionsRequest request)
    {
        var instructions = _reference.Instructions ?? Array.Empty<Instruction>();
        if (request.Instruction is null || string.IsNullOrWhiteSpace(request.Instruction))
        {
            return new InstructionsResult(Status.SUCCESS, new List<string>(), instructions);
        }

        var match = instructions
            .Where(entry => entry.Name.Equals(request.Instruction.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (match.Length == 0)
        {
            return new InstructionsResult(
                Status.NOT_FOUND,
                new List<string> { $"Unknown MIPS instruction '{request.Instruction}'." },
                Array.Empty<Instruction>());
        }

        return new InstructionsResult(Status.SUCCESS, new List<string>(), match);
    }
}
