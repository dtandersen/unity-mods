using System;
using System.Collections.Generic;

namespace Sage.Window;

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
        _current.ToggleRequested += OnToggleRequested;
        _current.UpdateRequested += OnUpdateRequested;
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

            _current.ToggleRequested -= OnToggleRequested;
            _current.UpdateRequested -= OnUpdateRequested;
            _current = next;
            _current.ToggleRequested += OnToggleRequested;
            _current.UpdateRequested += OnUpdateRequested;
        }
    }

    public void SetLines(IReadOnlyList<string> lines)
    {
        lock (_gate)
        {
            _current.SetLines(lines);
        }
    }

    public void SetStatus(string status)
    {
        lock (_gate)
        {
            _current.SetStatus(status);
        }
    }

    public void SetVisible(bool visible)
    {
        lock (_gate)
        {
            _current.SetVisible(visible);
        }
    }

    public event Action? ToggleRequested;

    public event Action? UpdateRequested;

    private void OnToggleRequested()
    {
        ToggleRequested?.Invoke();
    }

    private void OnUpdateRequested()
    {
        UpdateRequested?.Invoke();
    }
}
