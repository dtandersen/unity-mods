using System;
using System.Threading;
using System.Threading.Tasks;
using OdinsAi.Formatting;
using OdinsAi.Time;

namespace OdinsAi.Ai
{
    /// <summary>
    /// Presents an assistant turn to chat: echoes the player's question, decides
    /// which of the assistant's activity to show, and posts the answer. Chat writes
    /// go through <see cref="IChatSink"/>, so this flow is testable without Unity.
    /// </summary>
    /// <remarks>
    /// The presenter implements <see cref="IAssistantActivity"/>: reasoning, tool
    /// calls and tool output are shown only when their option is enabled. While the
    /// assistant works, if a piece of activity is not shown and no chat output has
    /// appeared for <see cref="ThinkingDelay"/>, a <c>Thinking...</c> status is
    /// posted instead, repeating for each further quiet interval.
    /// </remarks>
    public sealed class ChatPresenter : IAssistantActivity
    {
        /// <summary>Quiet period before a <c>Thinking...</c> status is posted.</summary>
        public static readonly TimeSpan ThinkingDelay = TimeSpan.FromSeconds(10);

        private readonly IChatSink _chat;
        private readonly Func<IAssistantActivity, AiAssistant> _assistantFactory;
        private readonly Func<string> _playerName;
        private readonly string _assistantName;
        private readonly Action<string> _log;
        private readonly bool _showThinking;
        private readonly bool _showToolCalls;
        private readonly bool _showToolResults;
        private readonly IClock _clock;
        private DateTimeOffset _lastEvent;
        private bool _hasActivity;

        /// <param name="chat">Destination for chat lines.</param>
        /// <param name="assistantFactory">
        /// Builds the assistant, or returns null when game data is not loaded. It
        /// receives the activity sink to report reasoning and tool use to.
        /// </param>
        /// <param name="playerName">Supplies the local player's name for the echo.</param>
        /// <param name="assistantName">Name the assistant posts under.</param>
        /// <param name="log">Receives exception details.</param>
        /// <param name="showThinking">Post the model's reasoning and narration.</param>
        /// <param name="showToolCalls">Post each requested tool call.</param>
        /// <param name="showToolResults">Post each tool's raw output.</param>
        /// <param name="clock">Time source; defaults to the system clock.</param>
        public ChatPresenter(
            IChatSink chat,
            Func<IAssistantActivity, AiAssistant> assistantFactory,
            Func<string> playerName = null,
            string assistantName = null,
            Action<string> log = null,
            bool showThinking = false,
            bool showToolCalls = false,
            bool showToolResults = false,
            IClock clock = null)
        {
            _chat = chat;
            _assistantFactory = assistantFactory;
            _playerName = playerName;
            _assistantName = assistantName;
            _log = log;
            _showThinking = showThinking;
            _showToolCalls = showToolCalls;
            _showToolResults = showToolResults;
            _clock = clock ?? SystemClock.Instance;
            _lastEvent = _clock.UtcNow;
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

                var assistant = _assistantFactory?.Invoke(this);
                if (assistant == null)
                {
                    Say("game data is not loaded yet.");
                    return;
                }

                var answer = await AnswerAsync(assistant, prompt, conversation, cancellationToken);
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
                var assistant = _assistantFactory?.Invoke(this);
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
            Write(ChatNaming.Apply(_assistantName, text));
        }

        /// <inheritdoc/>
        public void OnReasoning(string text)
        {
            if (_showThinking)
            {
                Say(text);
            }

            NoteActivity();
        }

        /// <inheritdoc/>
        public void OnToolCall(string text)
        {
            if (_showToolCalls)
            {
                Say(text);
            }

            NoteActivity();
        }

        /// <inheritdoc/>
        public void OnToolOutput(string text)
        {
            if (_showToolResults)
            {
                Say(text);
            }

            NoteActivity();
        }

        private async Task<string> AnswerAsync(
            AiAssistant assistant,
            string prompt,
            Conversation conversation,
            CancellationToken cancellationToken)
        {
            var task = assistant.AskAsync(prompt, conversation, cancellationToken);

            // While the assistant works, a quiet chat gets a placeholder, but only
            // once there has been something to wait for (reasoning or a tool call).
            while (!task.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var delay = _clock.Delay(ThinkingDelay, cancellationToken);
                if (await Task.WhenAny(task, delay) == task)
                {
                    break;
                }

                if (_hasActivity && _clock.UtcNow - _lastEvent >= ThinkingDelay)
                {
                    Say("Thinking...");
                }
            }

            return await task;
        }

        private void Echo(string prompt)
        {
            var name = _playerName?.Invoke();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "You";
            }

            Write($"{name}: {prompt}");
        }

        private void NoteActivity()
        {
            _hasActivity = true;
            _lastEvent = _clock.UtcNow;
        }

        private void Write(string line)
        {
            _chat.Write(line);
            _lastEvent = _clock.UtcNow;
        }
    }
}