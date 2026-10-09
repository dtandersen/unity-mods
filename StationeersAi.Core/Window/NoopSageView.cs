using System;
using System.Collections.Generic;

namespace StationeersAi.Window;

/// <summary>
/// A view that drops all output. The presenter drives it until the real
/// window loads, so turns can run with no window on screen.
/// </summary>
public sealed class NoopSageView : ISageView
{
    public void SetLines(IReadOnlyList<string> lines)
    {
    }

    public void SetStatus(string status)
    {
    }

    public void SetVisible(bool visible)
    {
    }

    // Never fires; subscribers come and go with no effect.
    public event Action? ToggleRequested { add { } remove { } }

    // Never fires; subscribers come and go with no effect.
    public event Action? UpdateRequested { add { } remove { } }
}
