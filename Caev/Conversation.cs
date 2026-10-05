using System.Collections.Generic;
using System.Text;

namespace Caev;

/// <summary>
/// Conversation history carried across questions so the model can refer back to
/// earlier turns. Every message persists verbatim — user and assistant text
/// plus tool calls and their results, like a standard agent transcript — and
/// the oldest turns drop whole once the byte budget runs out. The budget is
/// in bytes, not messages: one tool-heavy question can hold more text than
/// three simple ones.
/// </summary>
public sealed class Conversation
{
    /// <summary>Framing counted per message (role and envelope), in bytes.</summary>
    internal const int MessageOverheadBytes = 32;

    private readonly List<ChatMessage> _messages = new List<ChatMessage>();
    private readonly int _maxBytes;

    public Conversation(int maxBytes = 65536)
    {
        _maxBytes = maxBytes < 1 ? 1 : maxBytes;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public int MaxBytes => _maxBytes;

    /// <summary>Bytes currently held, by the same measure as the budget.</summary>
    public int SizeBytes
    {
        get
        {
            var total = 0;
            foreach (var message in _messages)
            {
                total += SizeOf(message);
            }

            return total;
        }
    }

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

    internal static int SizeOf(ChatMessage message)
    {
        var size = MessageOverheadBytes;
        size += ByteCount(message.Content);
        size += ByteCount(message.ToolCallId);
        if (message.ToolCalls != null)
        {
            foreach (var call in message.ToolCalls)
            {
                size += ByteCount(call.Id);
                size += ByteCount(call.Name);
                if (call.Arguments?.Values != null)
                {
                    foreach (var pair in call.Arguments.Values)
                    {
                        size += ByteCount(pair.Key);
                        size += ByteCount(pair.Value);
                    }
                }
            }
        }

        return size;
    }

    private static int ByteCount(string? text)
    {
        return string.IsNullOrEmpty(text) ? 0 : Encoding.UTF8.GetByteCount(text);
    }

    private void Trim()
    {
        // The newest message always survives: a lone turn over budget is
        // kept rather than dropped, so the current question keeps context.
        while (SizeBytes > _maxBytes && _messages.Count > 1)
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
