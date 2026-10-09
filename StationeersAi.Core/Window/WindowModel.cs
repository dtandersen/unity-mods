using System.Collections.Generic;
using Caev;

namespace StationeersAi.Window;

/// <summary>
/// The chat history behind the Sage window. A dumb store: appends lines
/// and drops the oldest past the cap. Thread-safe, so answers can land
/// from any thread while the view reads its snapshot on the main thread.
/// All decisions (busy, visibility) live in
/// <see cref="SagePresenter"/>; the view only renders this state.
/// </summary>
public sealed class WindowModel
{
    private readonly List<string> _lines = new List<string>();
    private readonly object _gate = new object();

    public WindowModel(int maxLines)
    {
        MaxLines = maxLines < 1 ? 1 : maxLines;
    }

    public int MaxLines { get; }

    /// <summary>Snapshot of the history, safe to read from any thread.</summary>
    public IReadOnlyList<string> Lines
    {
        get
        {
            lock (_gate)
            {
                return _lines.ToArray();
            }
        }
    }

    public void Append(string? line)
    {
        lock (_gate)
        {
            _lines.Add(line ?? string.Empty);
            while (_lines.Count > MaxLines)
            {
                _lines.RemoveAt(0);
            }
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _lines.Clear();
        }
    }
}
