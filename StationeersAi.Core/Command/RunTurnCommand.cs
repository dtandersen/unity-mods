using System;
using System.Threading;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Core;
using StationeersAi.Formatting;
using Caev.Tools;

namespace StationeersAi.Command;

/// <summary>One assistant turn: the prompt to run.</summary>
public sealed class RunTurnRequest
{
    public RunTurnRequest(string prompt)
    {
        Prompt = prompt ?? string.Empty;
    }

    public string Prompt { get; }
}

/// <summary>
/// Runs one assistant turn and reports it through events: reasoning, tool
/// calls and tool output as they happen, the plain-text answer at the end.
/// The game-not-loaded notice and errors arrive as answers too, so the turn
/// itself never throws. Callers decide what to show: the window and console
/// turns display reasoning and tool traffic per their options, mentions only
/// post the answer.
/// </summary>
public sealed class RunTurnCommand
{
    /// <summary>The model's reasoning and narration, as it happens.</summary>
    public event Action<string>? Think;

    /// <summary>Each requested tool call, as it happens.</summary>
    public event Action<string>? ToolCall;

    /// <summary>Each tool's output, as it happens.</summary>
    public event Action<string>? ToolOutput;

    /// <summary>The turn's answer: plain text, error, or game-not-loaded notice.</summary>
    public event Action<string>? Answered;

    private readonly Func<IAssistantActivity?, AiAssistant?> _assistantFactory;
    private readonly Logger _log;

    /// <param name="assistantFactory">
    /// Builds the assistant, or returns null when game data is not loaded. It
    /// receives the activity sink to report reasoning and tool use to.
    /// </param>
    /// <param name="log">Receives exception details.</param>
    public RunTurnCommand(Func<IAssistantActivity?, AiAssistant?> assistantFactory, Logger log)
    {
        _assistantFactory = assistantFactory;
        _log = log;
    }

    /// <param name="request">The turn to run.</param>
    /// <param name="conversation">History sent with the request. May be null.</param>
    /// <param name="activity">
    /// Extra activity sink (a presenter showing per its options). May be null.
    /// </param>
    public async Task ExecuteAsync(
        RunTurnRequest request,
        Conversation? conversation,
        IAssistantActivity? activity = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = request != null ? request.Prompt : string.Empty;
        try
        {
            var assistant = _assistantFactory?.Invoke(new EventActivity(this, activity));
            if (assistant == null)
            {
                OnAnswered("game data is not loaded yet.");
                return;
            }

            var answer = await assistant.AskAsync(prompt, conversation, cancellationToken).ConfigureAwait(false);
            OnAnswered(MarkdownToPlain.ToPlainText(answer));
        }
        catch (Exception ex)
        {
            OnAnswered($"error: {ex.Message}");
            _log.Log(ex.ToString());
        }
    }

    private void OnAnswered(string text)
    {
        Answered?.Invoke(text);
    }

    /// <summary>Fans assistant activity out to the caller and the events.</summary>
    private sealed class EventActivity : IAssistantActivity
    {
        private readonly RunTurnCommand _owner;
        private readonly IAssistantActivity? _inner;

        public EventActivity(RunTurnCommand owner, IAssistantActivity? inner)
        {
            _owner = owner;
            _inner = inner;
        }

        public void OnReasoning(string text)
        {
            _inner?.OnReasoning(text);
            _owner.Think?.Invoke(text);
        }

        public void OnToolCall(string text)
        {
            _inner?.OnToolCall(text);
            _owner.ToolCall?.Invoke(text);
        }

        public void OnToolOutput(string text)
        {
            _inner?.OnToolOutput(text);
            _owner.ToolOutput?.Invoke(text);
        }
    }
}
