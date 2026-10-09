using System;

namespace Caev;

/// <summary>
/// Everything one agent turn reports, as events you can subscribe to: reasoning,
/// tool calls, tool output, and the answer at the end. Whoever runs the turn owns
/// one instance, subscribes to what it shows, and passes it to the assistant.
/// </summary>
/// <remarks>
/// The assistant reports what happened; the subscriber decides whether to show it
/// (and when to show a placeholder instead). Raise methods, not interface calls,
/// so a test double can report the same traffic.
/// </remarks>
public sealed class AgentEvents
{
    /// <summary>The model's reasoning or the narration accompanying a tool call.</summary>
    public event Action<string>? OnReasoning;

    /// <summary>A tool the model asked to run, formatted as <c>name(args)</c>.</summary>
    public event Action<string>? OnToolCall;

    /// <summary>The raw result a tool returned.</summary>
    public event Action<string>? OnToolOutput;

    /// <summary>The answer, as the model wrote it (Markdown). Raised when the turn ends; the subscriber renders it.</summary>
    public event Action<string>? OnAnswer;

    public void RaiseReasoning(string text)
    {
        OnReasoning?.Invoke(text);
    }

    public void RaiseToolCall(string text)
    {
        OnToolCall?.Invoke(text);
    }

    public void RaiseToolOutput(string text)
    {
        OnToolOutput?.Invoke(text);
    }

    public void RaiseAnswer(string text)
    {
        OnAnswer?.Invoke(text);
    }
}
