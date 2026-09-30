using System;
using StationeersAi.Ai;

namespace StationeersAi.Mod.Ai
{
    /// <summary>
    /// Adapts in-game chat to <see cref="IChatSink"/>. Answers are posted as the
    /// assistant through <see cref="ChatSender"/>, marshalled to the Unity main
    /// thread: the presenter calls back on background continuations after the
    /// first await, and network sends are only safe on the main thread.
    /// </summary>
    public sealed class ChatMessageSink : IChatSink
    {
        private readonly Action<Action> _runOnMainThread;

        public ChatMessageSink(Action<Action> runOnMainThread)
        {
            _runOnMainThread = runOnMainThread;
        }

        public void Write(string line)
        {
            var text = line ?? string.Empty;
            _runOnMainThread(() => ChatSender.Send(BepInExPlugin.AssistantName, text));
        }
    }
}
