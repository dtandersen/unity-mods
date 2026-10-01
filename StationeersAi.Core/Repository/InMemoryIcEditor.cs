using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory IC editor. Used by tests.</summary>
    public sealed class InMemoryIcEditor : IIcEditor
    {
        public readonly List<(string Computer, string Code)> Writes = new List<(string Computer, string Code)>();

        /// <summary>The computer a write resolves to, or null when no computer matches.</summary>
        public string ResolveAs { get; set; } = "Lab Computer";

        /// <summary>The text reads return as the editor contents.</summary>
        public string EditorCode { get; set; } = string.Empty;

        public IcWriteResult Write(string computer, string code)
        {
            Writes.Add((computer, code));
            if (ResolveAs == null)
            {
                return new IcWriteResult(false, null, 0, $"Computer '{computer}' not found.");
            }

            return new IcWriteResult(true, ResolveAs, (code ?? string.Empty).Split('\n').Length, null);
        }

        public IcReadResult Read(string computer)
        {
            if (ResolveAs == null)
            {
                return new IcReadResult(false, null, null, $"Computer '{computer}' not found.");
            }

            return new IcReadResult(true, ResolveAs, EditorCode, null);
        }
    }
}
