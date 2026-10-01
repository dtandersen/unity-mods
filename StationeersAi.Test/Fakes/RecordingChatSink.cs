using System.Collections.Generic;
using StationeersAi.Ai;

namespace StationeersAi.Test.Fakes;

/// <summary>Records chat lines instead of writing them to the game.</summary>
public sealed class RecordingChatSink : IChatSink
{
    public List<string> Lines { get; } = new List<string>();

    public void Write(string line)
    {
        Lines.Add(line);
    }
}
