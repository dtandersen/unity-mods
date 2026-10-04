using System;
using System.Collections.Generic;

namespace StationeersAi.Window;

/// <summary>
/// A view that drops all output. The presenter drives it until the real
/// window loads, so turns can run with no window on screen.
/// </summary>
public sealed class NoopSageView : ISageView
{
    public string Input { get; set; } = string.Empty;

    public void SetLines(IReadOnlyList<string> lines)
    {
    }

    public void SetBusy(bool busy)
    {
    }

    public void SetVisible(bool visible)
    {
    }

    public void RequestFocus()
    {
    }

    // Never fire; subscribers come and go with no effect.
    public event Action? SubmitRequested { add { } remove { } }

    public event Action? ToggleRequested { add { } remove { } }
}
