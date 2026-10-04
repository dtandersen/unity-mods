using System.Collections.Generic;
using Caev;

namespace StationeersAi.Window;

/// <summary>
/// The chat history behind the Sage window. A dumb store: appends lines
/// and drops the oldest past the cap. Thread-safe, so answers can land
/// from any thread while the view reads its snapshot on the main thread.
/// All decisions (submit, busy, visibility) live in
/// <see cref="SagePresenter"/>; the view only renders this state.
/// </summary>
public sealed class WindowModel
{
    private readonly List<string> _lines = new List<string>();
    private readonly object _gate = new object();
    private int _thinkingIndex = -1;

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
                if (_thinkingIndex == 0)
                {
                    _thinkingIndex = -1;
                }
                else if (_thinkingIndex > 0)
                {
                    _thinkingIndex--;
                }
            }
        }
    }

    /// <summary>
    /// Shows the thinking status, overwriting itself: the first call appends
    /// it, later calls replace it in place. Only one turn runs at a time,
    /// so the tracked line is always the right one.
    /// </summary>
    public void SetThinking(string line)
    {
        lock (_gate)
        {
            if (_thinkingIndex < 0 || _thinkingIndex >= _lines.Count)
            {
                _lines.Add(line ?? string.Empty);
                _thinkingIndex = _lines.Count - 1;
            }
            else
            {
                _lines[_thinkingIndex] = line ?? string.Empty;
            }
        }
    }

    /// <summary>Removes the thinking status. Returns false when none is shown.</summary>
    public bool ClearThinking()
    {
        lock (_gate)
        {
            if (_thinkingIndex < 0 || _thinkingIndex >= _lines.Count)
            {
                _thinkingIndex = -1;
                return false;
            }

            _lines.RemoveAt(_thinkingIndex);
            _thinkingIndex = -1;
            return true;
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _lines.Clear();
            _thinkingIndex = -1;
        }
    }
}
