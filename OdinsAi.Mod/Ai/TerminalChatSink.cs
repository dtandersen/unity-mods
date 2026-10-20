using System.Reflection;
using OdinsAi.Ai;
using UnityEngine;

namespace OdinsAi.Mod.Ai
{
    /// <summary>Adapts Valheim's <see cref="Terminal"/> to <see cref="IChatSink"/>.</summary>
    public sealed class TerminalChatSink : IChatSink
    {
        // Chat.Update() keeps the chat window active while m_hideTimer < m_hideDelay,
        // so resetting the timer to zero pops the window open.
        private static readonly FieldInfo ChatHideTimer =
            typeof(Chat).GetField("m_hideTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Terminal _terminal;

        public TerminalChatSink(Terminal terminal)
        {
            _terminal = terminal;
        }

        public void Write(string line)
        {
            _terminal.AddString(line);
            OpenChatWindow();
        }

        private static void OpenChatWindow()
        {
            if (Chat.instance != null && ChatHideTimer != null)
            {
                ChatHideTimer.SetValue(Chat.instance, 0f);
            }
        }
    }
}