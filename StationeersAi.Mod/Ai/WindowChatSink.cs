using System;
using StationeersAi.Ai;

namespace StationeersAi.Mod.Ai
{
    /// <summary>
    /// Adapts the in-game Sage window to <see cref="IChatSink"/>. Appends lines
    /// to the window history: the presenter calls back on background
    /// continuations, and the window drains them under lock on the main thread.
    /// </summary>
    public sealed class WindowChatSink : IChatSink
    {
        private readonly Action<string> _append;

        public WindowChatSink(Action<string> append)
        {
            _append = append;
        }

        public void Write(string line)
        {
            _append(line ?? string.Empty);
        }
    }
}
