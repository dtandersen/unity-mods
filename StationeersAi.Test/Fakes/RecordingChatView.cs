using System.Collections.Generic;
using StationeersAi.View;

namespace StationeersAi.Test.Fakes;

/// <summary>
/// Records what chat was shown instead of sending it: one line per post, with the
/// name the game would show, so specs assert what a player reads.
/// </summary>
public sealed class RecordingChatView : IChatView
{
    public List<string> Lines { get; } = new List<string>();

    public void Post(string name, string text)
    {
        Lines.Add($"{name}: {text}");
    }
}
