using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Core;
using StationeersAi.Tool;

namespace StationeersAi.Ai
{
    /// <summary>
    /// Runs the model/tool loop: ask the model, execute any tool calls it requests,
    /// feed the results back, repeat until it produces a final answer.
    /// </summary>
    /// <remarks>
    /// The optional callbacks keep the core free of any logging/UI framework:
    /// <paramref name="log"/> receives the full trace (plugin logs it) and
    /// <paramref name="activity"/> receives reasoning, tool calls and tool output.
    /// <paramref name="systemPrompt"/> and <paramref name="activity"/> may be null
    /// (no system prompt / no activity sink). The implementor of
    /// <see cref="IAssistantActivity"/> decides what to show.
    /// </remarks>
    public sealed class AiAssistant
    {
        private readonly IAiClient _client;
        private readonly ToolHandler _tools;
        private readonly string _systemPrompt;
        private readonly int _maxIterations;
        private readonly Logger _log;
        private readonly IAssistantActivity _activity;

        public AiAssistant(
            IAiClient client,
            ToolHandler tools,
            string systemPrompt,
            int maxIterations,
            Logger log,
            IAssistantActivity activity)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _systemPrompt = systemPrompt;
            _maxIterations = maxIterations;
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _activity = activity;
        }

        /// <summary>
        /// Answers a question. If <paramref name="conversation"/> is non-null, prior
        /// turns are sent with the request and this turn is appended to it.
        /// </summary>
        public async Task<string> AskAsync(
            string prompt,
            Conversation conversation,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(_systemPrompt))
            {
                messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));
            }

            if (conversation != null)
            {
                messages.AddRange(conversation.Messages);
            }

            messages.Add(new ChatMessage(ChatRole.User, prompt));
            Log($"You: {prompt}");

            for (var i = 0; i < _maxIterations; i++)
            {
                var completion = await _client.CompleteAsync(messages, _tools.Definitions, cancellationToken);

                if (!string.IsNullOrEmpty(completion?.Content))
                {
                    Log($"Sage thinking: {completion.Content}");
                }

                if (!string.IsNullOrEmpty(completion?.Reasoning))
                {
                    Log($"Sage thinking: {Readable(completion.Reasoning)}");
                    _activity?.OnReasoning(completion.Reasoning);
                }

                if (completion == null || !completion.HasToolCalls)
                {
                    var answer = completion?.Content ?? string.Empty;
                    conversation?.Add(new ChatMessage(ChatRole.User, prompt));
                    conversation?.Add(new ChatMessage(ChatRole.Assistant, answer));
                    if (!string.IsNullOrEmpty(answer))
                    {
                        Log($"Sage: {answer}");
                    }

                    Log($"(done in {i + 1} {(i == 0 ? "turn" : "turns")})");
                    return answer;
                }

                if (!string.IsNullOrEmpty(completion.Content))
                {
                    _activity?.OnReasoning(completion.Content);
                }

                messages.Add(new ChatMessage(ChatRole.Assistant, completion.Content, completion.ToolCalls));

                foreach (var call in completion.ToolCalls)
                {
                    var described = $"{call.Name}({Describe(call.Arguments)})";
                    Log($"Sage runs {described}");
                    _activity?.OnToolCall(described);

                    var result = _tools.Handle(call);
                    Log($"Result:\n{result.Content}");
                    _activity?.OnToolOutput(result.Content);
                    messages.Add(new ChatMessage(ChatRole.Tool, result.Content, ToolCallId: call.Id));
                }
            }

            Log($"(gave up after {_maxIterations} tool rounds)");
            return "I couldn't complete that request.";
        }

        /// <summary>
        /// Condenses the conversation into a single summary message, using the model,
        /// so older context can be dropped without losing the important facts.
        /// </summary>
        public async Task<string> CompactAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            if (conversation == null || conversation.Messages.Count == 0)
            {
                return null;
            }

            var transcript = string.Join("\n", conversation.Messages.Select(m => $"{m.Role}: {m.Content}"));
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System,
                    "Summarise the conversation below in a short paragraph. Preserve names, numbers and facts the user may refer back to."),
                new ChatMessage(ChatRole.User, transcript),
            };

            Log("compacting conversation...");
            var completion = await _client.CompleteAsync(messages, Array.Empty<ToolDefinition>(), cancellationToken);
            var summary = completion?.Content;

            if (!string.IsNullOrWhiteSpace(summary))
            {
                conversation.Replace(new[] { new ChatMessage(ChatRole.Assistant, summary) });
                Log("compacted conversation to a single summary message");
            }

            return summary;
        }

        private void Log(string message)
        {
            _log.Log(message);
        }

        private static string Describe(ToolArguments arguments)
        {
            return arguments?.Values == null
                ? string.Empty
                : string.Join(", ", arguments.Values.Select(pair => $"{pair.Key}={Summarize(pair.Value)}"));
        }

        /// <summary>One-line argument for the log: long values collapse to a size.</summary>
        private static string Summarize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var lines = value.Split('\n');
            if (lines.Length > 4)
            {
                return $"<{lines.Length} lines>";
            }

            var flat = value.Replace("\r", string.Empty).Replace("\n", "\\n");
            return flat.Length > 80 ? flat.Substring(0, 80) + "\u2026" : flat;
        }

        /// <summary>Reasoning as one thought per line, for the log only.</summary>
        private static string Readable(string text)
        {
            var spaced = System.Text.RegularExpressions.Regex.Replace(text ?? string.Empty, @"\.(?=[A-Z])", ". ");
            return System.Text.RegularExpressions.Regex.Replace(spaced, @"\. (?=[A-Z])", ".\n");
        }
    }
}
