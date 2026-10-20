using System;
using OdinsAi.Ai;
using UnityEngine;

namespace OdinsAi.Mod.Ai
{
    /// <summary>
    /// Broadcasts a chat line from the server using Valheim's built-in global
    /// "ChatMessage" RPC, so unmodded clients display it. There is no bot identity,
    /// so the line is attributed to the asking player's <see cref="UserInfo"/>.
    /// The write is marshalled to the main thread.
    /// </summary>
    public sealed class ServerChatSink : IChatSink
    {
        private readonly UserInfo _sender;
        private readonly Vector3 _position;
        private readonly Action<Action> _onMainThread;

        public ServerChatSink(UserInfo sender, Vector3 position, Action<Action> onMainThread)
        {
            _sender = sender;
            _position = position;
            _onMainThread = onMainThread;
        }

        public void Write(string line)
        {
            _onMainThread(() =>
            {
                var rpc = ZRoutedRpc.instance;
                if (rpc != null)
                {
                    // Everybody (0) makes the server broadcast to every peer.
                    rpc.InvokeRoutedRPC(
                        ZRoutedRpc.Everybody,
                        "ChatMessage",
                        _position,
                        (int)Talker.Type.Normal,
                        _sender,
                        line);
                }
            });
        }
    }
}