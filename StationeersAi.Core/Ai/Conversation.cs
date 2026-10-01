using System.Collections.Generic;

namespace StationeersAi.Ai
{
    /// <summary>
    /// Conversation history carried across questions so the model can refer back to
    /// earlier turns. Every message persists verbatim — user and assistant text
    /// plus tool calls and their results, like a standard agent transcript — and
    /// the oldest turns drop whole once the size limit is exceeded.
    /// </summary>
    public sealed class Conversation
    {
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();
        private readonly int _maxMessages;

        public Conversation(int maxMessages = 20)
        {
            _maxMessages = maxMessages < 2 ? 2 : maxMessages;
        }

        public IReadOnlyList<ChatMessage> Messages => _messages;

        public int MaxMessages => _maxMessages;

        public void Add(ChatMessage message)
        {
            _messages.Add(message);
            Trim();
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public void Replace(IEnumerable<ChatMessage> messages)
        {
            _messages.Clear();
            _messages.AddRange(messages);
            Trim();
        }

        private void Trim()
        {
            while (_messages.Count > _maxMessages && _messages.Count > 0)
            {
                if (_messages[0].Role != ChatRole.User)
                {
                    _messages.RemoveAt(0);
                    continue;
                }

                var nextTurn = _messages.FindIndex(1, message => message.Role == ChatRole.User);
                if (nextTurn < 0)
                {
                    _messages.RemoveAt(0);
                    continue;
                }

                _messages.RemoveRange(0, nextTurn);
            }
        }
    }
}
