using System;
using System.Collections.Generic;
using StationeersAi.Window;

namespace StationeersAi.Test.Fakes;

/// <summary>Passive-view fake: records what the presenter sets, raises gestures.</summary>
public sealed class FakeSageView : ISageView
{
    public string Input { get; set; } = string.Empty;

    public bool Visible { get; private set; }

    public bool Busy { get; private set; }

    public List<string> Lines { get; private set; } = new List<string>();

    public int FocusRequests { get; private set; }

    public event Action SubmitRequested;

    public event Action ToggleRequested;

    public void SetLines(IReadOnlyList<string> lines)
    {
        Lines = lines != null ? new List<string>(lines) : new List<string>();
    }

    public void SetBusy(bool busy)
    {
        Busy = busy;
    }

    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    public void RequestFocus()
    {
        FocusRequests++;
    }

    public void RaiseSubmit()
    {
        SubmitRequested?.Invoke();
    }

    public void RaiseToggle()
    {
        ToggleRequested?.Invoke();
    }
}
