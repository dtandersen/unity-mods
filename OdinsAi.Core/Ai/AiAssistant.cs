using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OdinsAi.Tool;

namespace OdinsAi.Ai
{
    /// <summary>
    /// Runs the model/tool loop: ask the model, execute any tool calls it requests,
    /// feed the results back, repeat until it produces a final answer.
    /// </summary>
    public sealed class AiAssistant
    {
        private readonly IAiClient _client;
        private readonly ToolHandler _tools;
        private readonly string _systemPrompt;
        private readonly int _maxIterations;

        public AiAssistant(IAiClient client, ToolHandler tools, string systemPrompt = null, int maxIterations = 5)
        {
            _client = client;
            _tools = tools;
            _systemPrompt = systemPrompt;
            _maxIterations = maxIterations;
        }

        public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(_systemPrompt))
            {
                messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));
            }

            messages.Add(new ChatMessage(ChatRole.User, prompt));

            for (var i = 0; i < _maxIterations; i++)
            {
                var completion = await _client.CompleteAsync(messages, _tools.Definitions, cancellationToken);

                if (completion == null || !completion.HasToolCalls)
                {
                    return completion?.Content ?? string.Empty;
                }

                messages.Add(new ChatMessage(ChatRole.Assistant, completion.Content, completion.ToolCalls));

                foreach (var call in completion.ToolCalls)
                {
                    var result = _tools.Handle(call);
                    messages.Add(new ChatMessage(ChatRole.Tool, result.Content, ToolCallId: call.Id));
                }
            }

            return "I couldn't complete that request.";
        }
    }
}