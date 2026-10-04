using System;
using System.Collections.Generic;

namespace StationeersAi.Window;

/// <summary>
/// The Sage window as a passive view: it renders what the
/// <see cref="SagePresenter"/> sets and reports user gestures back as
/// events. It holds no rules — no trimming, no busy checks, no history
/// cap. Unity-free, so specs drive it through a fake.
/// </summary>
public interface ISageView
{
    /// <summary>The text field. The presenter reads it on submit and clears it after.</summary>
    string Input { get; set; }

    /// <summary>Replaces the rendered history with a snapshot from the model.</summary>
    void SetLines(IReadOnlyList<string> lines);

    /// <summary>Shows the Send button as busy ("...") while true.</summary>
    void SetBusy(bool busy);

    /// <summary>Shows or hides the window.</summary>
    void SetVisible(bool visible);

    /// <summary>
    /// Moves keyboard focus to the input on the next frame. Only sets a
    /// flag; the Unity view consumes it where focus calls are legal.
    /// </summary>
    void RequestFocus();

    /// <summary>Raised when the user presses Send or Enter in the input.</summary>
    event Action? SubmitRequested;

    /// <summary>Raised when the user presses the window toggle key.</summary>
    event Action? ToggleRequested;
}
