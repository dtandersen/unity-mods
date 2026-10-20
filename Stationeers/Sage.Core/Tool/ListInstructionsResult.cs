using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>IC10 instructions from the game's editor help.</summary>
public sealed record ListInstructionsResult(IReadOnlyList<Instruction> Instructions);
