using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory MIPS reference. Used by tests.</summary>
public sealed class InMemoryMipsReference : IMipsReference
{
    public List<Instruction> Instructions { get; } = new List<Instruction>();

    IReadOnlyList<Instruction>? IMipsReference.Instructions => Instructions;
}
