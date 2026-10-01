using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory MIPS reference. Used by tests.</summary>
    public sealed class InMemoryMipsReference : IMipsReference
    {
        public List<MipsInstruction> Instructions { get; } = new List<MipsInstruction>();

        IReadOnlyList<MipsInstruction> IMipsReference.Instructions => Instructions;
    }
}
