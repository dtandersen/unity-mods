using System;
using System.Collections.Generic;

namespace StationeersAi.Window;

/// <summary>
/// The Sage window as a passive view: it renders what the
/// <see cref="SagePresenter"/> sets and reports user gestures back as
/// events. It holds no rules — no busy checks, no history
/// cap. Unity-free, so specs drive it through a fake.
/// </summary>
public interface ISageView
{
    /// <summary>Replaces the rendered history with a snapshot from the model.</summary>
    void SetLines(IReadOnlyList<string> lines);

    /// <summary>Replaces the status bar text ("Idle" or the thinking status).</summary>
    void SetStatus(string status);

    /// <summary>Shows or hides the window.</summary>
    void SetVisible(bool visible);

    /// <summary>Raised when the user presses the window toggle key.</summary>
    event Action? ToggleRequested;

    /// <summary>
    /// Raised on every update tick (each Unity frame). The presenter uses it
    /// to refresh the status bar with the elapsed thinking time in real time.
    /// </summary>
    event Action? UpdateRequested;
}
