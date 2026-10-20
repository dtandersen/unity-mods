using System.Collections.Generic;

namespace OdinsAi.Ai
{
    /// <summary>
    /// Conversation history carried across questions so the model can refer back to
    /// earlier turns. Only user/assistant text is retained (not tool traffic), and
    /// the oldest messages are dropped once the size limit is exceeded.
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
            while (_messages.Count > _maxMessages)
            {
                _messages.RemoveAt(0);
            }
        }
    }
}