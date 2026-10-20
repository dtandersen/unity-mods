using System;
using OdinsAi.Ai;

namespace OdinsAi.Mod.Ai
{
    /// <summary>
    /// Writes a line to shared chat, so every player sees it. The write is
    /// marshalled to the Unity main thread, since answers arrive on a continuation
    /// from the HTTP request.
    /// </summary>
    public sealed class BroadcastChatSink : IChatSink
    {
        private readonly Action<Action> _onMainThread;

        public BroadcastChatSink(Action<Action> onMainThread)
        {
            _onMainThread = onMainThread;
        }

        public void Write(string line)
        {
            _onMainThread(() =>
            {
                if (Chat.instance != null && Player.m_localPlayer != null)
                {
                    Chat.instance.SendText(Talker.Type.Normal, line);
                }
            });
        }
    }
}