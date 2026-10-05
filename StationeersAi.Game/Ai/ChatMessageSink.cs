using StationeersAi.Core;
using System;
using Caev;

namespace StationeersAi.Game.Ai;

/// <summary>
/// Adapts in-game chat to <see cref="IChatSink"/>. Answers are posted as the
/// assistant through <see cref="ChatSender"/>, marshalled to the Unity main
/// thread: the presenter calls back on background continuations after the
/// first await, and network sends are only safe on the main thread.
/// </summary>
public sealed class ChatMessageSink : IChatSink
{
    private readonly IMainThread _threads;
    private readonly Logger _log;
    private readonly Func<string> _assistantName;
    private readonly ChatSender _sender;

    public ChatMessageSink(IMainThread mainThread, Logger log, Func<string> assistantName)
    {
        _threads = mainThread;
        _log = log;
        _assistantName = assistantName;
        _sender = new ChatSender(log);
    }

    public void Write(string line)
    {
        var text = line ?? string.Empty;
        _log.Info($"chat sink queued: {Truncate(text, 80)}");
        _threads.Run(() => _sender.Send(_assistantName(), text));
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
        {
            return value;
        }

        return value.Substring(0, max) + "...";
    }
}
