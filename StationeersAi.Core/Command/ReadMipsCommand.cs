using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: look up IC10 instructions by mnemonic.</summary>
public sealed class ReadMipsCommand
{
    private readonly IMipsReference _reference;

    public ReadMipsCommand(IMipsReference reference)
    {
        _reference = reference;
    }

    /// <summary>
    /// All instructions when <paramref name="instruction"/> is blank, else
    /// the one matching mnemonic (empty when unknown).
    /// </summary>
    public IReadOnlyList<MipsInstruction> Execute(string? instruction)
    {
        var instructions = _reference.Instructions ?? Array.Empty<MipsInstruction>();
        if (instruction is null || string.IsNullOrWhiteSpace(instruction))
        {
            return instructions;
        }

        return instructions
            .Where(entry => entry.Name.Equals(instruction.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
