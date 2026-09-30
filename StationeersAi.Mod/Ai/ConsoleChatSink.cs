using System;
using StationeersAi.Ai;

namespace StationeersAi.Mod.Ai
{
    /// <summary>
    /// Adapts Stationeers' console window to <see cref="IChatSink"/>. Writes are
    /// marshalled to the Unity main thread: the presenter calls back on background
    /// continuations after the first await.
    /// </summary>
    public sealed class ConsoleChatSink : IChatSink
    {
        private readonly Action<Action> _runOnMainThread;

        public ConsoleChatSink(Action<Action> runOnMainThread)
        {
            _runOnMainThread = runOnMainThread;
        }

        public void Write(string line)
        {
            var text = line ?? string.Empty;
            _runOnMainThread(() => PrintLines(text));
        }

        private static void PrintLines(string text)
        {
            // The console draws one entry per call; split multi-line answers so
            // each line renders instead of collapsing the layout.
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            foreach (var line in lines)
            {
                Assets.Scripts.ConsoleWindow.Print(
                    line, ConsoleColor.Gray, clearLine: false, aged: false, unformatted: true);
            }
        }
    }
}
