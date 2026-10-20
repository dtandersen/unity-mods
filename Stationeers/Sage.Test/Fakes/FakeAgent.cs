#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caev;
using Sage.Core;

namespace Sage.Test.Fakes;

/// <summary>
/// Scripted <see cref="AgentGateway"/> for specs about the turn, not the loop.
/// Reports the scripted reasoning and tool traffic through the turn's
/// <see cref="AgentEvents"/>, then reports the next scripted answer, as the
/// assistant does when it finishes.
/// </summary>
public sealed class FakeAgent : AgentGateway
{
    private readonly AgentEvents _events;
    private readonly Queue<string> _answers = new Queue<string>();
    private readonly Queue<string?> _summaries = new Queue<string?>();
    private readonly List<string> _reasoning = new List<string>();
    private readonly List<string> _toolCalls = new List<string>();
    private readonly List<string> _toolOutputs = new List<string>();

    public FakeAgent(AgentEvents events)
    {
        _events = events;
    }

    /// <summary>The channel this gateway reports to.</summary>
    public AgentEvents Events => _events;

    /// <summary>Every prompt asked, in order.</summary>
    public List<string> Prompts { get; } = new List<string>();

    /// <summary>The conversation each ask received, in order (null when none).</summary>
    public List<Conversation?> Conversations { get; } = new List<Conversation?>();

    /// <summary>Every turn handed to the gateway, in order.</summary>
    public List<TurnContext> Turns { get; } = new List<TurnContext>();

    /// <summary>How many compactions were asked for.</summary>
    public int CompactCalls { get; private set; }

    public FakeAgent ReportsReasoning(string text)
    {
        _reasoning.Add(text);
        return this;
    }

    public FakeAgent ReportsToolCall(string text)
    {
        _toolCalls.Add(text);
        return this;
    }

    public FakeAgent ReportsToolOutput(string text)
    {
        _toolOutputs.Add(text);
        return this;
    }

    public FakeAgent AnswersWith(string answer)
    {
        _answers.Enqueue(answer);
        return this;
    }

    public FakeAgent CompactsTo(string? summary)
    {
        _summaries.Enqueue(summary);
        return this;
    }

    public Task AskAsync(
        TurnContext turn,
        CancellationToken cancellationToken = default)
    {
        Prompts.Add(turn.Prompt);
        Conversations.Add(turn.Conversation);
        Turns.Add(turn);

        foreach (var text in _reasoning)
        {
            _events.RaiseReasoning(text);
        }

        foreach (var text in _toolCalls)
        {
            _events.RaiseToolCall(text);
        }

        foreach (var text in _toolOutputs)
        {
            _events.RaiseToolOutput(text);
        }

        var answer = _answers.Count > 0 ? _answers.Dequeue() : "(no scripted answer)";
        _events.RaiseAnswer(answer);
        return Task.CompletedTask;
    }

    public Task<string?> CompactAsync(
        Conversation? conversation,
        CancellationToken cancellationToken = default)
    {
        CompactCalls++;
        return Task.FromResult(_summaries.Count > 0 ? _summaries.Dequeue() : null);
    }
}
