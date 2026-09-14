using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OdinsAi.Tool;

namespace OdinsAi.Ai
{
    /// <summary>
    /// Runs the model/tool loop: ask the model, execute any tool calls it requests,
    /// feed the results back, repeat until it produces a final answer.
    /// </summary>
    /// <remarks>
    /// The optional callbacks keep the core free of any logging/UI framework:
    /// <paramref name="log"/> receives the full trace (plugin logs it) and
    /// <paramref name="thinking"/> receives the user-facing intermediate steps
    /// (the plugin can post them to chat).
    /// </remarks>
    public sealed class AiAssistant
    {
        private readonly IAiClient _client;
        private readonly ToolHandler _tools;
        private readonly string _systemPrompt;
        private readonly int _maxIterations;
        private readonly Action<string> _log;
        private readonly Action<string> _thinking;

        public AiAssistant(
            IAiClient client,
            ToolHandler tools,
            string systemPrompt = null,
            int maxIterations = 5,
            Action<string> log = null,
            Action<string> thinking = null)
        {
            _client = client;
            _tools = tools;
            _systemPrompt = systemPrompt;
            _maxIterations = maxIterations;
            _log = log;
            _thinking = thinking;
        }

        public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(_systemPrompt))
            {
                messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));
            }

            messages.Add(new ChatMessage(ChatRole.User, prompt));
            Log($"question: {prompt}");

            for (var i = 0; i < _maxIterations; i++)
            {
                var completion = await _client.CompleteAsync(messages, _tools.Definitions, cancellationToken);

                if (!string.IsNullOrEmpty(completion?.Content))
                {
                    Log($"assistant: {completion.Content}");
                }

                if (completion == null || !completion.HasToolCalls)
                {
                    Log($"done after {i + 1} turn(s)");
                    return completion?.Content ?? string.Empty;
                }

                // Intermediate reasoning text (if the model produced any).
                if (!string.IsNullOrEmpty(completion.Content))
                {
                    Thinking(completion.Content);
                }

                messages.Add(new ChatMessage(ChatRole.Assistant, completion.Content, completion.ToolCalls));

                foreach (var call in completion.ToolCalls)
                {
                    var described = $"{call.Name}({Describe(call.Arguments)})";
                    Log($"tool call: {described}");
                    Thinking(described);

                    var result = _tools.Handle(call);
                    Log($"tool result: {result.Content}");
                    messages.Add(new ChatMessage(ChatRole.Tool, result.Content, ToolCallId: call.Id));
                }
            }

            Log($"gave up: reached MaxToolIterations ({_maxIterations}) without a final answer");
            return "I couldn't complete that request.";
        }

        private void Log(string message)
        {
            _log?.Invoke(message);
        }

        private void Thinking(string message)
        {
            _thinking?.Invoke(message);
        }

        private static string Describe(ToolArguments arguments)
        {
            return arguments?.Values == null
                ? string.Empty
                : string.Join(", ", arguments.Values.Select(pair => $"{pair.Key}={pair.Value}"));
        }
    }
}