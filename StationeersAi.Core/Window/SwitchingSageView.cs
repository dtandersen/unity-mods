using System;
using System.Collections.Generic;

namespace StationeersAi.Window;

/// <summary>
/// A view that forwards to the current target. It starts on a noop view;
/// the mod points it at the real window on load and back on unload, so one
/// presenter outlives the window.
/// </summary>
public sealed class SwitchingSageView : ISageView
{
    private readonly object _gate = new object();
    private readonly NoopSageView _noop = new NoopSageView();
    private ISageView _current;

    public SwitchingSageView()
    {
        _current = _noop;
        _current.SubmitRequested += OnSubmitRequested;
        _current.ToggleRequested += OnToggleRequested;
    }

    /// <summary>Points the view at the next target. Null points it at noop.</summary>
    public void Show(ISageView? view)
    {
        lock (_gate)
        {
            var next = view ?? (ISageView)_noop;
            if (ReferenceEquals(next, _current))
            {
                return;
            }

            _current.SubmitRequested -= OnSubmitRequested;
            _current.ToggleRequested -= OnToggleRequested;
            _current = next;
            _current.SubmitRequested += OnSubmitRequested;
            _current.ToggleRequested += OnToggleRequested;
        }
    }

    public string Input
    {
        get { lock (_gate) return _current.Input; }
        set { lock (_gate) _current.Input = value; }
    }

    public void SetLines(IReadOnlyList<string> lines)
    {
        lock (_gate)
        {
            _current.SetLines(lines);
        }
    }

    public void SetBusy(bool busy)
    {
        lock (_gate)
        {
            _current.SetBusy(busy);
        }
    }

    public void SetVisible(bool visible)
    {
        lock (_gate)
        {
            _current.SetVisible(visible);
        }
    }

    public void RequestFocus()
    {
        lock (_gate)
        {
            _current.RequestFocus();
        }
    }

    public event Action? SubmitRequested;

    public event Action? ToggleRequested;

    private void OnSubmitRequested()
    {
        SubmitRequested?.Invoke();
    }

    private void OnToggleRequested()
    {
        ToggleRequested?.Invoke();
    }
}
