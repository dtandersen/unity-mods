using System;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;

namespace StationeersAi.Window;

/// <summary>
/// Presents Sage turns in the window: owns the <see cref="WindowModel"/>,
/// answers the <see cref="ISageView"/> events, and pushes every state change
/// to the view. Each turn echoes the question, runs through
/// <see cref="RunTurnCommand"/>, and shows reasoning and tool traffic per
/// its options. This is also the <see cref="IChatSink"/> the mention answers
/// stream into, so the window keeps the full history.
/// </summary>
/// <remarks>
/// The view is only touched through <paramref name="marshal"/>, which the
/// plugin wires to its main-thread dispatcher; specs run it inline. The
/// model stays readable from any thread. Display options are read per turn,
/// so config edits apply without a restart.
/// </remarks>
public sealed class SagePresenter : IChatSink
{
    private readonly WindowModel _model;
    private readonly ISageView _view;
    private readonly Func<IAssistantActivity?, AiAssistant?> _assistantFactory;
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
    private DateTimeOffset _turnStarted;

    /// <param name="model">History store. Never null.</param>
    /// <param name="view">Passive view to drive. Never null.</param>
    /// <param name="assistantFactory">
    /// Builds the assistant, or returns null when game data is not loaded. It
    /// receives the activity sink to report reasoning and tool use to.
    /// </param>
    /// <param name="playerName">Supplies the local player's name for the echo. May return null, which echoes as "You".</param>
    /// <param name="assistantName">Supplies the name the assistant posts under. May return null, which posts as "Sage".</param>
    /// <param name="log">Receives exception details.</param>
    /// <param name="showThinking">Post the model's reasoning and narration.</param>
    /// <param name="showToolCalls">Post each requested tool call.</param>
    /// <param name="showToolResults">Post each tool's raw output.</param>
    /// <param name="conversation">Supplies the shared conversation. May return null, which sends no history.</param>
    /// <param name="marshal">
    /// Runs view updates where the view demands it (Unity's main thread).
    /// Defaults to running inline.
    /// </param>
    /// <param name="clock">Supplies the current time for the thinking status. Defaults to UTC now.</param>
    public SagePresenter(
        WindowModel model,
        ISageView view,
        Func<IAssistantActivity?, AiAssistant?> assistantFactory,
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
        _assistantFactory = assistantFactory;
        _playerName = playerName;
        _assistantName = assistantName;
        _log = log;
        _showThinking = showThinking;
        _showToolCalls = showToolCalls;
        _showToolResults = showToolResults;
        _conversation = conversation;
        _marshal = marshal ?? (action => action());
        _clock = clock ?? (() => DateTimeOffset.UtcNow);

        _view.SubmitRequested += OnSubmitRequested;
        _view.ToggleRequested += OnToggleRequested;
        PushAll();
    }

    /// <summary>Pushes full state to the view. Call after the view swaps.</summary>
    public void Refresh()
    {
        PushAll();
    }

    public bool Busy
    {
        get { lock (_gate) return _busy; }
    }

    public bool Visible
    {
        get { lock (_gate) return _visible; }
    }

    public void Show()
    {
        lock (_gate)
        {
            _visible = true;
        }

        Run(() => _view.SetVisible(true));
    }

    /// <summary>Opens the window and moves keyboard focus to its input.</summary>
    public void ShowAndFocus()
    {
        Show();
        Run(() => _view.RequestFocus());
    }

    public void Toggle()
    {
        bool visible;
        lock (_gate)
        {
            _visible = !_visible;
            visible = _visible;
        }

        Run(() =>
        {
            _view.SetVisible(visible);
            if (visible)
            {
                _view.RequestFocus();
            }
        });
    }

    /// <summary>
    /// Asks from outside the view (console command): opens the window and
    /// runs one turn. Empty and busy prompts are ignored.
    /// </summary>
    public Task AskAsync(string prompt)
    {
        Show();
        return StartTurn(prompt, clearInput: false);
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
            var assistant = _assistantFactory?.Invoke(null);
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
            var summary = await assistant.CompactAsync(conversation).ConfigureAwait(false);
            Say(string.IsNullOrWhiteSpace(summary)
                ? "compaction produced no summary."
                : "context compacted.");
        }
        catch (Exception ex)
        {
            Say($"error: {ex.Message}");
            _log.Log(ex.ToString());
        }
    }

    /// <summary>
    /// Sink for the mention answers: lines land here from any thread, join
    /// the model, and are pushed to the view.
    /// </summary>
    public void Write(string? line)
    {
        _model.Append(line);
        PushLines();
    }

    private void OnSubmitRequested()
    {
        _ = StartTurn(_view.Input, clearInput: true);
    }

    private Task StartTurn(string prompt, bool clearInput)
    {
        if (!TryBeginTurn(prompt, out var trimmed))
        {
            return Task.CompletedTask;
        }

        _turnStarted = _clock();

        if (clearInput)
        {
            Run(() => _view.Input = string.Empty);
        }

        Run(() => _view.SetBusy(true));
        return TurnAsync(trimmed);
    }

    private async Task TurnAsync(string prompt)
    {
        try
        {
            Echo(prompt);
            await RunCommandAsync(prompt, _conversation != null ? _conversation() : null).ConfigureAwait(false);
        }
        finally
        {
            ClearBusy();
        }
    }

    private async Task RunCommandAsync(string prompt, Conversation? conversation)
    {
        var command = new RunTurnCommand(_assistantFactory, _log);
        command.Think += text =>
        {
            if (_showThinking != null && _showThinking())
            {
                Say(text);
            }
            else
            {
                NoteProgress();
            }
        };
        command.ToolCall += text =>
        {
            if (_showToolCalls != null && _showToolCalls())
            {
                Say(text);
            }
            else
            {
                NoteProgress();
            }
        };
        command.ToolOutput += text =>
        {
            if (_showToolResults != null && _showToolResults())
            {
                Say(text);
            }
            else
            {
                NoteProgress();
            }
        };
        command.Answered += answer =>
        {
            ClearThinking();
            Say(answer);
        };
        await command.ExecuteAsync(new RunTurnRequest(prompt), conversation, null).ConfigureAwait(false);
    }

    /// <summary>Refreshes the thinking status with the elapsed turn time.</summary>
    private void NoteProgress()
    {
        var elapsed = _clock() - _turnStarted;
        var seconds = elapsed.TotalSeconds < 0 ? 0 : (int)elapsed.TotalSeconds;
        _model.SetThinking(ChatNaming.Apply(_assistantName != null ? _assistantName() : null, $"Thinking ({seconds}s)..."));
        PushLines();
    }

    private void ClearThinking()
    {
        if (_model.ClearThinking())
        {
            PushLines();
        }
    }

    private void PushLines()
    {
        var snapshot = _model.Lines;
        Run(() => _view.SetLines(snapshot));
    }

    /// <summary>Posts an assistant line to the window history.</summary>
    private void Say(string? text)
    {
        Write(ChatNaming.Apply(_assistantName != null ? _assistantName() : null, text));
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

    /// <summary>
    /// Claims one turn for a prompt: trims it and takes busy under one
    /// lock. Returns false (and takes nothing) when the prompt is empty
    /// or a turn is already running.
    /// </summary>
    private bool TryBeginTurn(string prompt, out string trimmed)
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
        }

        Run(() => _view.SetBusy(false));
    }

    private void PushAll()
    {
        var snapshot = _model.Lines;
        Run(() =>
        {
            _view.SetVisible(_visible);
            _view.SetBusy(_busy);
            _view.Input = string.Empty;
            _view.SetLines(snapshot);
        });
    }

    private void Run(Action action)
    {
        _marshal(action);
    }
}
