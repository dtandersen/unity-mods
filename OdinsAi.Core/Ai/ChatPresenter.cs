using System;
using System.Threading;
using System.Threading.Tasks;
using OdinsAi.Formatting;

namespace OdinsAi.Ai
{
    /// <summary>
    /// Presents an assistant turn to chat: echoes the player's question, posts a
    /// <c>Thinking...</c> status, then the answer. Chat writes go through
    /// <see cref="IChatSink"/>, so this flow is testable without Unity or a Terminal.
    /// </summary>
    public sealed class ChatPresenter
    {
        private readonly IChatSink _chat;
        private readonly Func<Action<string>, AiAssistant> _assistantFactory;
        private readonly Func<string> _playerName;
        private readonly string _assistantName;
        private readonly Action<string> _log;

        /// <param name="chat">Destination for chat lines.</param>
        /// <param name="assistantFactory">
        /// Builds the assistant, or returns null when game data is not loaded. It
        /// receives the action to use for the assistant's activity callbacks.
        /// </param>
        /// <param name="playerName">Supplies the local player's name for the echo.</param>
        /// <param name="assistantName">Name the assistant posts under.</param>
        /// <param name="log">Receives exception details.</param>
        public ChatPresenter(
            IChatSink chat,
            Func<Action<string>, AiAssistant> assistantFactory,
            Func<string> playerName = null,
            string assistantName = null,
            Action<string> log = null)
        {
            _chat = chat;
            _assistantFactory = assistantFactory;
            _playerName = playerName;
            _assistantName = assistantName;
            _log = log;
        }

        /// <summary>Answers a question, echoing it first so it stays in the log.</summary>
        public async Task AskAsync(
            string prompt,
            Conversation conversation = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Echo(prompt);

                var assistant = _assistantFactory?.Invoke(Say);
                if (assistant == null)
                {
                    Say("game data is not loaded yet.");
                    return;
                }

                Say("Thinking...");
                var answer = await assistant.AskAsync(prompt, conversation, cancellationToken);
                Say(MarkdownToTmp.ToTmpRichText(answer));
            }
            catch (AiClientException ex)
            {
                Say($"error: {ex.Message}");
                _log?.Invoke(ex.ToString());
            }
            catch (Exception ex)
            {
                Say($"error: {ex.Message}");
                _log?.Invoke(ex.ToString());
            }
        }

        /// <summary>Drops the conversation and reports how many messages remain.</summary>
        public void Clear(Conversation conversation)
        {
            conversation.Clear();
            Say($"context cleared ({conversation.Messages.Count} messages).");
        }

        /// <summary>Condenses the conversation into a single summary message.</summary>
        public async Task CompactAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            try
            {
                var assistant = _assistantFactory?.Invoke(Say);
                if (assistant == null)
                {
                    Say("game data is not loaded yet.");
                    return;
                }

                if (conversation.Messages.Count == 0)
                {
                    Say("nothing to compact.");
                    return;
                }

                Say("compacting context...");
                var summary = await assistant.CompactAsync(conversation, cancellationToken);
                Say(string.IsNullOrWhiteSpace(summary)
                    ? "compaction produced no summary."
                    : "context compacted.");
            }
            catch (Exception ex)
            {
                Say($"error: {ex.Message}");
                _log?.Invoke(ex.ToString());
            }
        }

        /// <summary>Posts an assistant line, prefixed with the assistant's name.</summary>
        public void Say(string text)
        {
            _chat.Write(ChatNaming.Apply(_assistantName, text));
        }

        private void Echo(string prompt)
        {
            var name = _playerName?.Invoke();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "You";
            }

            _chat.Write($"{name}: {prompt}");
        }
    }
}