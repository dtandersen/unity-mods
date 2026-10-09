using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caev;
using Caev.Tools;

namespace Caev.Test.Fakes;

/// <summary>
/// Scripted <see cref="IAiClient"/> for tests. Returns queued completions and
/// records every set of messages it receives so tests can assert on the
/// conversation (e.g. that a tool result was fed back to the model).
/// </summary>
public sealed class FakeAiClient : IAiClient
{
    private readonly Queue<AiCompletion> _scripted = new Queue<AiCompletion>();
    private readonly TaskCompletionSource<bool> _gate =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Name reported to the assistant; the fake answers in its place.</summary>
    public string Model { get; set; } = "fake";

    public List<IReadOnlyList<ChatMessage>> Transcript { get; } = new List<IReadOnlyList<ChatMessage>>();

    /// <summary>When true, once the script is exhausted, reply with the last tool result.</summary>
    public bool EchoLastToolResult { get; set; }

    /// <summary>When true, calls block until <see cref="Release"/> is called.</summary>
    public bool Hold { get; set; }

    private int? _failureStatus;

    /// <summary>Lets any held calls complete.</summary>
    public void Release()
    {
        _gate.TrySetResult(true);
    }

    public FakeAiClient RespondWith(AiCompletion completion)
    {
        _scripted.Enqueue(completion);
        return this;
    }

    /// <summary>Fails every later call as the service did with that HTTP status.</summary>
    public FakeAiClient FailWith(int? statusCode)
    {
        _failureStatus = statusCode;
        return this;
    }

    public async Task<AiCompletion> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        Transcript.Add(messages.ToArray());

        if (_failureStatus.HasValue)
        {
            throw new AiClientException(
                $"The AI service returned HTTP {_failureStatus.Value}: stub failure",
                _failureStatus.Value,
                null);
        }

        if (Hold)
        {
            await _gate.Task;
        }

        if (_scripted.Count > 0)
        {
            return _scripted.Dequeue();
        }

        if (EchoLastToolResult)
        {
            var lastTool = messages.LastOrDefault(message => message.Role == ChatRole.Tool);
            return new AiCompletion(lastTool?.Content ?? "(no tool result)", Array.Empty<ToolCall>());
        }

        return new AiCompletion("(no scripted response)", Array.Empty<ToolCall>());
    }
}
