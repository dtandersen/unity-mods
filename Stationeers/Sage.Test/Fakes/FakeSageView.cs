using System;
using System.Collections.Generic;
using Sage.Window;

namespace Sage.Test.Fakes;

/// <summary>Passive-view fake: records what the presenter sets, raises gestures.</summary>
public sealed class FakeSageView : ISageView
{
    public bool Visible { get; private set; }

    public string Status { get; private set; } = "Idle";

    public List<string> Lines { get; private set; } = new List<string>();

    public event Action ToggleRequested;

    public event Action UpdateRequested;

    public void SetLines(IReadOnlyList<string> lines)
    {
        Lines = lines != null ? new List<string>(lines) : new List<string>();
    }

    public void SetStatus(string status)
    {
        Status = status ?? "Idle";
    }

    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    public void RaiseToggle()
    {
        ToggleRequested?.Invoke();
    }

    public void RaiseUpdate()
    {
        UpdateRequested?.Invoke();
    }
}
