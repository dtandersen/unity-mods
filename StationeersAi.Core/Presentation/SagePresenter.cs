using System;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Ai;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;
using StationeersAi.View;
using StationeersAi.Window;

namespace StationeersAi.Presentation;

/// <summary>
/// Presents the agent to the stationeer on both surfaces: the Sage window and
/// game chat. It owns the <see cref="WindowModel"/>, answers the
/// <see cref="ISageView"/> gestures, takes chat lines through
/// <see cref="OnChatLine"/>, and runs one turn at a time through
/// <see cref="RunTurnCommand"/>. Reasoning and tool traffic go to the window per
/// its options; every answer goes to the window and to chat. The window has no
/// input: questions arrive through chat or the console command, and the status
/// bar shows whether a turn runs.
/// </summary>
/// <remarks>
/// It is the only object that starts a turn, so its busy flag is the single gate:
/// a question that arrives while one runs is refused and logged. Whether a chat
/// line asks something is <see cref="ChatRouting"/>'s call, not this object's; it
/// is handed the decision's inputs. The window view is only touched through
/// <paramref name="marshal"/>, which the plugin wires to Unity's main thread;
/// specs run it inline. Display options are read per turn, so config edits apply
/// without a restart.
/// </remarks>
public sealed class SagePresenter
{
    private const string IdleStatus = "Idle";

    private readonly WindowModel _model;
    private readonly ISageView _view;
    private readonly IChatView _chat;
    private readonly AgentGateway _gateway;
    private readonly AgentSettings _settings;
    private readonly Func<string?> _playerName;
    private readonly Func<string?> _assistantName;
    private readonly Logger _log;
    private readonly Func<bool> _showThinking;
    private readonly Func<bool> _showToolCalls;
    private readonly Func<bool> _showToolResults;
    private readonly Func<Conversation?> _conversation;
    private readonly Action<Action> _marshal;
    private readonly Func<DateTimeOffset> _clock;
    private readonly object _gate = new object();
    private bool _busy;
    private bool _visible;
    private string _status = IdleStatus;
    private DateTimeOffset _turnStarted;

    /// <param name="model">History store. Never null.</param>
    /// <param name="view">Passive window view to drive. Never null.</param>
    /// <param name="chat">Passive chat view to post answers through. Never null.</param>
    /// <param name="gateway">The agent to ask, and the channel it reports to.
    /// The presenter subscribes to that channel once, here.</param>
    /// <param name="settings">Instructions and iteration limit for each turn, from the game's config.</param>
    /// <param name="playerName">Supplies the local player's name for the echo. May return null, which echoes as "You".</param>
    /// <param name="assistantName">Supplies the name the agent posts under. May return null, which posts as "Sage".</param>
    /// <param name="log">Receives seen chat lines, refusals, and exception details.</param>
    /// <param name="showThinking">Show the model's reasoning and narration in the window.</param>
    /// <param name="showToolCalls">Show each requested tool call in the window.</param>
    /// <param name="showToolResults">Show each tool's raw output in the window.</param>
    /// <param name="conversation">Supplies the shared conversation. May return null, which sends no history.</param>
    /// <param name="marshal">
    /// Runs view updates where the window view demands it (Unity's main thread).
    /// Defaults to running inline.
    /// </param>
    /// <param name="clock">Supplies the current time for the thinking status. Defaults to UTC now.</param>
    public SagePresenter(
        WindowModel model,
        ISageView view,
        IChatView chat,
        AgentGateway gateway,
        AgentSettings settings,
        Func<string?> playerName,
        Func<string?> assistantName,
        Logger log,
        Func<bool> showThinking,
        Func<bool> showToolCalls,
        Func<bool> showToolResults,
        Func<Conversation?> conversation,
        Action<Action>? marshal = null,
        Func<DateTimeOffset>? clock = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _chat = chat ?? throw new ArgumentNullException(nameof(chat));
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _settings = settings;
        _playerName = playerName;
        _assistantName = assistantName;
        _log = log;
        _showThinking = showThinking;
        _showToolCalls = showToolCalls;
        _showToolResults = showToolResults;
        _conversation = conversation;
        _marshal = marshal ?? (action => action());
        _clock = clock ?? (() => DateTimeOffset.UtcNow);

        _view.ToggleRequested += OnToggleRequested;
        _view.UpdateRequested += OnUpdateRequested;

        // One gateway, one channel: subscribe for the life of the presenter.
        _gateway.Events.OnReasoning += OnReasoning;
        _gateway.Events.OnToolCall += OnToolCall;
        _gateway.Events.OnToolOutput += OnToolOutput;
        _gateway.Events.OnAnswer += OnAnswered;

        PushAll();
    }

    /// <summary>Pushes full state to the view. Call after the view swaps.</summary>
    public void Refresh()
    {
        PushAll();
    }

    /// <summary>Whether a turn is running on either surface.</summary>
    public bool Busy
    {
        get { lock (_gate) return _busy; }
    }

    public bool Visible
    {
        get { lock (_gate) return _visible; }
    }

    /// <summary>
    /// Takes one chat line the game displayed, with what routing needs to know.
    /// Shows the window, echoes the question, and runs the turn when the line asks
    /// something, returning that turn. Returns a completed task when the line is
    /// refused, noted only, or already waiting on another turn.
    /// </summary>
    public Task OnChatLine(ChatLine line, ChatRoutingRules rules)
    {
        if (line == null || rules == null)
        {
            return Task.CompletedTask;
        }

        var decision = ChatRouting.Decide(line, rules);
        if (!decision.Considered)
        {
            return Task.CompletedTask;
        }

        _log.Info($"chat seen from '{line.DisplayName}': {Truncate(line.Text, 80)}");

        if (!decision.ShouldAnswer || !TryBeginTurn(decision.Prompt, out var prompt))
        {
            if (decision.ShouldAnswer)
            {
                _log.Info("chat ignored: Sage is already answering");
            }

            return Task.CompletedTask;
        }

        _log.Info(decision.Mentioned
            ? $"mention matched, asking: {Truncate(prompt, 80)}"
            : $"direct chat asking: {Truncate(prompt, 80)}");

        Show();
        Write($"{Speaker(line.DisplayName)}: {prompt}");
        _turnStarted = _clock();
        RefreshThinkingStatus();
        return TurnAsync(prompt, echo: false);
    }

    /// <summary>Runs one console line split into words. Never throws.</summary>
    public string OnConsoleCommand(string[] args)
    {
        if (args != null && args.Length > 0)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "window":
                    Show();
                    return "Sage window opened.";

                case "reset":
                    ClearConversation();
                    return "Context cleared.";

                case "compact":
                    _ = CompactConversation();
                    return "Compacting context...";
            }
        }

        var prompt = args != null ? string.Join(" ", args).Trim() : string.Empty;
        if (prompt.Length == 0)
        {
            return "Usage: ai <question>  (also: ai reset, ai compact)";
        }

        _ = AskAsync(prompt);
        return "Question queued";
    }

    public void Show()
    {
        lock (_gate)
        {
            _visible = true;
        }

        Run(() => _view.SetVisible(true));
    }

    public void Toggle()
    {
        bool visible;
        lock (_gate)
        {
            _visible = !_visible;
            visible = _visible;
        }

        Run(() => _view.SetVisible(visible));
    }

    /// <summary>
    /// Asks from outside the view (console command): opens the window and
    /// runs one turn. Empty and busy prompts are ignored.
    /// </summary>
    public Task AskAsync(string prompt)
    {
        Show();
        return StartTurn(prompt);
    }

    /// <summary>Drops the shared conversation and reports it in the window.</summary>
    public void ClearConversation()
    {
        var conversation = _conversation != null ? _conversation() : null;
        if (conversation == null)
        {
            return;
        }

        conversation.Clear();
        Say($"context cleared ({conversation.Messages.Count} messages).");
    }

    /// <summary>Condenses the shared conversation, reporting progress in the window.</summary>
    public async Task CompactConversation()
    {
        var conversation = _conversation != null ? _conversation() : null;
        if (conversation == null)
        {
            return;
        }

        try
        {
            if (conversation.Messages.Count == 0)
            {
                Say("nothing to compact.");
                return;
            }

            Say("compacting context...");
            var summary = await _gateway.CompactAsync(conversation).ConfigureAwait(false);
            Say(string.IsNullOrWhiteSpace(summary)
                ? "compaction produced no summary."
                : "context compacted.");
        }
        catch (Exception ex)
        {
            Say($"error: {ex.Message}");
            _log.Info(ex.ToString());
        }
    }

    /// <summary>
    /// Adds one line to the window history, from any thread, and pushes the new
    /// snapshot to the view.
    /// </summary>
    public void Write(string? line)
    {
        _model.Append(line);
        PushLines();
    }

    private Task StartTurn(string prompt)
    {
        if (!TryBeginTurn(prompt, out var trimmed))
        {
            return Task.CompletedTask;
        }

        _turnStarted = _clock();
        RefreshThinkingStatus();
        return TurnAsync(trimmed, echo: true);
    }

    /// <summary>
    /// Runs one turn for a prompt. <paramref name="echo"/> writes the local
    /// player's line first; a chat question is already echoed by its speaker.
    /// </summary>
    private async Task TurnAsync(string prompt, bool echo)
    {
        try
        {
            if (echo)
            {
                Echo(prompt);
            }

            await RunCommandAsync(prompt, _conversation != null ? _conversation() : null).ConfigureAwait(false);
        }
        finally
        {
            ClearBusy();
        }
    }

    private async Task RunCommandAsync(string prompt, Conversation? conversation)
    {
        await new RunTurnCommand(_gateway, _settings, _log)
            .ExecuteAsync(new RunTurnRequest(prompt), conversation)
            .ConfigureAwait(false);
    }

    /// <summary>The model's reasoning, shown in the window per <c>General/ShowThinking</c>.</summary>
    private void OnReasoning(string text)
    {
        if (_showThinking != null && _showThinking())
        {
            Say(text);
        }
        else
        {
            NoteProgress();
        }
    }

    /// <summary>A tool call, shown in the window per <c>General/ShowToolCalls</c>.</summary>
    private void OnToolCall(string text)
    {
        if (_showToolCalls != null && _showToolCalls())
        {
            Say(text);
        }
        else
        {
            NoteProgress();
        }
    }

    /// <summary>A tool result, shown in the window per <c>General/ShowToolResults</c>.</summary>
    private void OnToolOutput(string text)
    {
        if (_showToolResults != null && _showToolResults())
        {
            Say(text);
        }
        else
        {
            NoteProgress();
        }
    }

    /// <summary>
    /// A turn ended: show the answer in the window, and
    /// post it to chat. Both surfaces get plain text; chat carries no name prefix,
    /// because the game shows the name it is posted under.
    /// </summary>
    private void OnAnswered(string answer)
    {
        var plain = MarkdownToPlain.ToPlainText(answer);
        Say(plain);
        _chat.Post(ChatNaming.Resolve(_assistantName != null ? _assistantName() : null), plain);
    }

    /// <summary>Refreshes the status bar with the elapsed turn time.</summary>
    private void NoteProgress()
    {
        RefreshThinkingStatus();
    }

    /// <summary>
    /// The update tick: while a turn runs, refreshes the status bar with the
    /// elapsed thinking time in real time. Idle ticks do nothing, so the
    /// status stays <c>Idle</c> when no turn runs.
    /// </summary>
    private void OnUpdateRequested()
    {
        lock (_gate)
        {
            if (!_busy)
            {
                return;
            }
        }

        RefreshThinkingStatus();
    }

    /// <summary>
    /// Pushes the current thinking text, but only when it changed since the
    /// last push, so the update tick does not queue a view write every frame.
    /// </summary>
    private void RefreshThinkingStatus()
    {
        var text = ThinkingText();
        lock (_gate)
        {
            if (!_busy || text == _status)
            {
                return;
            }

            _status = text;
        }

        Run(() => _view.SetStatus(text));
    }

    private string ThinkingText()
    {
        var elapsed = _clock() - _turnStarted;
        var seconds = elapsed.TotalSeconds < 0 ? 0 : elapsed.TotalSeconds;
        return ChatNaming.Apply(_assistantName != null ? _assistantName() : null, $"Thinking ({seconds:0.0}s)...");
    }

    private void SetStatus(string status)
    {
        lock (_gate)
        {
            _status = status ?? IdleStatus;
            status = _status;
        }

        Run(() => _view.SetStatus(status));
    }

    private void PushLines()
    {
        var snapshot = _model.Lines;
        Run(() => _view.SetLines(snapshot));
    }

    /// <summary>Posts an agent line to the window history.</summary>
    private void Say(string? text)
    {
        Write(ChatNaming.Apply(_assistantName != null ? _assistantName() : null, text));
    }

    /// <summary>Echoes the local player's question; a blank name shows as "You".</summary>
    private void Echo(string prompt)
    {
        var name = _playerName?.Invoke();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "You";
        }

        Write($"{name}: {prompt}");
    }

    /// <summary>The name a chat line is shown under: the poster's, or "player" when it has none.</summary>
    private static string Speaker(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName) ? "player" : displayName!.Trim();
    }

    /// <summary>
    /// Claims one turn for a prompt: trims it and takes busy under one
    /// lock. Returns false (and takes nothing) when the prompt is empty
    /// or a turn is already running.
    /// </summary>
    private bool TryBeginTurn(string? prompt, out string trimmed)
    {
        trimmed = prompt != null ? prompt.Trim() : string.Empty;
        if (trimmed.Length == 0)
        {
            return false;
        }

        lock (_gate)
        {
            if (_busy)
            {
                return false;
            }

            _busy = true;
            return true;
        }
    }

    private void OnToggleRequested()
    {
        Toggle();
    }

    private void ClearBusy()
    {
        lock (_gate)
        {
            _busy = false;
            _status = IdleStatus;
        }

        Run(() => _view.SetStatus(IdleStatus));
    }

    private void PushAll()
    {
        var snapshot = _model.Lines;
        string status;
        lock (_gate)
        {
            status = _status;
        }

        Run(() =>
        {
            _view.SetVisible(_visible);
            _view.SetStatus(status);
            _view.SetLines(snapshot);
        });
    }

    private void Run(Action action)
    {
        _marshal(action);
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value!.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
