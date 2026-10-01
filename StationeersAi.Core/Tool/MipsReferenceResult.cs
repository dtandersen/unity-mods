using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>IC10 instructions from the game's editor help.</summary>
public sealed record MipsReferenceResult(IReadOnlyList<MipsInstruction> Instructions);
