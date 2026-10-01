using System;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Ai;

namespace StationeersAi.Window;

/// <summary>
/// Presenter for the Sage window (passive view): owns the
/// <see cref="WindowModel"/>, answers the <see cref="ISageView"/> events,
/// and pushes every state change to the view. The view never touches the
/// model and decides nothing. This is also the <see cref="IChatSink"/> the
/// assistant writes to, so the player's echo and the answer land in the
/// window history.
/// </summary>
/// <remarks>
/// The view is only touched through <paramref name="marshal"/>, which the
/// plugin wires to its main-thread dispatcher; specs run it inline. The
/// model stays readable from any thread.
/// </remarks>
public sealed class SagePresenter : IChatSink
{
    private readonly WindowModel _model;
    private readonly ISageView _view;
    private readonly Func<string, Task> _ask;
    private readonly Action<Action> _marshal;
    private readonly object _gate = new object();
    private bool _busy;
    private bool _visible;

    /// <param name="model">History store. Never null.</param>
    /// <param name="view">Passive view to drive. Never null.</param>
    /// <param name="ask">
    /// Handles a submitted question (usually a <see cref="ChatPresenter"/>
    /// turn). May return null when there is nothing to run; the window
    /// still clears busy. May be null in specs that only check view wiring.
    /// </param>
    /// <param name="marshal">
    /// Runs view updates where the view demands it (Unity's main thread).
    /// Defaults to running inline.
    /// </param>
    public SagePresenter(
        WindowModel model,
        ISageView view,
        Func<string, Task> ask,
        Action<Action> marshal = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _ask = ask;
        _marshal = marshal ?? (action => action());

        _view.SubmitRequested += OnSubmitRequested;
        _view.ToggleRequested += OnToggleRequested;
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

        Run(() =>
        {
            _view.SetVisible(true);
            _view.RequestFocus();
        });
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
    /// Runs one assistant turn for a prompt from outside the view (a chat
    /// mention, a console question): same rules as a submit — empty and
    /// busy prompts are ignored — and the view shows busy until
    /// <paramref name="turn"/> finishes. Returns false when nothing ran.
    /// </summary>
    public async Task<bool> RunTurnAsync(string prompt, Func<string, Task> turn)
    {
        if (!TryBeginTurn(prompt, out prompt))
        {
            return false;
        }

        Run(() => _view.SetBusy(true));
        try
        {
            var task = turn != null ? turn(prompt) : null;
            if (task != null)
            {
                await task.ConfigureAwait(false);
            }
        }
        finally
        {
            ClearBusy();
        }

        return true;
    }

    /// <summary>
    /// Sink for the assistant: answers land here from any thread, join the
    /// model, and are pushed to the view.
    /// </summary>
    public void Write(string line)
    {
        _model.Append(line);
        var snapshot = _model.Lines;
        Run(() => _view.SetLines(snapshot));
    }

    private void OnSubmitRequested()
    {
        if (!TryBeginTurn(_view.Input, out var prompt))
        {
            return;
        }

        Run(() =>
        {
            _view.Input = string.Empty;
            _view.SetBusy(true);
        });

        Task task = null;
        try
        {
            task = _ask != null ? _ask(prompt) : null;
        }
        catch
        {
            ClearBusy();
            throw;
        }

        if (task == null)
        {
            ClearBusy();
            return;
        }

        task.ContinueWith(_ => ClearBusy(), TaskScheduler.Default);
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
