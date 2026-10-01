using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory IC editor. Used by tests.</summary>
    public sealed class InMemoryIcEditor : IIcEditor
    {
        public readonly List<(string Device, string Code)> Writes = new List<(string Device, string Code)>();

        /// <summary>The device a write resolves to, or null when no device matches.</summary>
        public string ResolveAs { get; set; } = "Lab Computer";

        public IcWriteResult Write(string device, string code)
        {
            Writes.Add((device, code));
            if (ResolveAs == null)
            {
                return new IcWriteResult(false, null, 0, $"Computer '{device}' not found.");
            }

            return new IcWriteResult(true, ResolveAs, (code ?? string.Empty).Split('\n').Length, null);
        }
    }
}
