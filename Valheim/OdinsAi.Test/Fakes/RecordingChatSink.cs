using System.Collections.Generic;
using OdinsAi.Ai;

namespace OdinsAi.Test.Fakes
{
    /// <summary>Records the lines written to chat so steps can assert on them.</summary>
    public sealed class RecordingChatSink : IChatSink
    {
        public List<string> Lines { get; } = new List<string>();

        public void Write(string line)
        {
            Lines.Add(line);
        }
    }
}