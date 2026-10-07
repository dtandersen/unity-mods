using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>The IC10 instruction reference from the game's editor help.</summary>
public interface IMipsReference
{
    /// <summary>Every known instruction, sorted by name.</summary>
    IReadOnlyList<Instruction>? Instructions { get; }
}
