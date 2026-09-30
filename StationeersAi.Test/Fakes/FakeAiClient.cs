using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Ai;
using StationeersAi.Tool;

namespace StationeersAi.Test.Fakes
{
    /// <summary>
    /// Scripted <see cref="IAiClient"/> for tests. Returns queued completions and
    /// records every set of messages it receives so tests can assert on the
    /// conversation (e.g. that a tool result was fed back to the model).
    /// </summary>
    public sealed class FakeAiClient : IAiClient
    {
        private readonly Queue<AiCompletion> _scripted = new Queue<AiCompletion>();

        public List<IReadOnlyList<ChatMessage>> Transcript { get; } = new List<IReadOnlyList<ChatMessage>>();

        /// <summary>When true, once the script is exhausted, reply with the last tool result.</summary>
        public bool EchoLastToolResult { get; set; }

        public FakeAiClient RespondWith(AiCompletion completion)
        {
            _scripted.Enqueue(completion);
            return this;
        }

        public Task<AiCompletion> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            IReadOnlyList<ToolDefinition> tools,
            CancellationToken cancellationToken = default)
        {
            Transcript.Add(messages.ToArray());

            if (_scripted.Count > 0)
            {
                return Task.FromResult(_scripted.Dequeue());
            }

            if (EchoLastToolResult)
            {
                var lastTool = messages.LastOrDefault(message => message.Role == ChatRole.Tool);
                return Task.FromResult(new AiCompletion(lastTool?.Content ?? "(no tool result)", Array.Empty<ToolCall>()));
            }

            return Task.FromResult(new AiCompletion("(no scripted response)", Array.Empty<ToolCall>()));
        }
    }
}
