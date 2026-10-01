using System;

namespace StationeersAi.Ai;

/// <summary>
/// Sends one chat line to several sinks, in order: the Sage window
/// presenter and public game chat receive the same content. Unity-free;
/// each sink marshals to the main thread itself.
/// </summary>
public sealed class FanoutChatSink : IChatSink
{
    private readonly IChatSink[] _sinks;

    public FanoutChatSink(params IChatSink[] sinks)
    {
        _sinks = sinks ?? Array.Empty<IChatSink>();
    }

    /// <summary>Writes <paramref name="line"/> to every sink, in order.</summary>
    public void Write(string line)
    {
        foreach (var sink in _sinks)
        {
            if (sink != null)
            {
                sink.Write(line);
            }
        }
    }
}
