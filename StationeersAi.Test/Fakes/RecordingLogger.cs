using System.Collections.Generic;
using StationeersAi.Core;

namespace StationeersAi.Test.Fakes
{
    /// <summary>Records core trace lines instead of writing them to the game log.</summary>
    public sealed class RecordingLogger : Logger
    {
        public List<string> Lines { get; } = new List<string>();

        public void Log(string message)
        {
            Lines.Add(message);
        }
    }
}
