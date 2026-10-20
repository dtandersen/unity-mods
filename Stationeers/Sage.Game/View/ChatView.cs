using System;
using Sage.Core;
using Sage.Game.Ai;
using Sage.View;
using Logger = Sage.Core.Logger;

namespace Sage.Game.View;

/// <summary>
/// The game's chat as a view: posts one named line through
/// <see cref="ChatSender"/>, marshalled to the Unity main thread. The presenter
/// calls back on background continuations after the first await, and network
/// sends are only safe on the main thread.
/// </summary>
public sealed class ChatView : IChatView
{
    private readonly IMainThread _threads;
    private readonly Logger _log;
    private readonly ChatSender _sender;

    public ChatView(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
        _sender = new ChatSender(log);
    }

    /// <summary>
    /// Posts <paramref name="text"/> as <paramref name="name"/>. The game shows the
    /// name, so the text carries no prefix.
    /// </summary>
    public void Post(string name, string text)
    {
        var line = text ?? string.Empty;
        _log.Info($"chat post queued: {Truncate(line, 80)}");
        _threads.Run(() => _sender.Send(name, line));
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
