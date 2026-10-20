using System.Collections.Generic;
using Sage.Core;
using Crowbus.Common;

namespace Sage.Test.Fakes;

/// <summary>Records core trace lines instead of writing them to the game log.</summary>
public sealed class RecordingLogger : Logger
{
    public List<string> Lines { get; } = new List<string>();

    public void Info(string message)
    {
        Lines.Add(message);
    }

    public void Warn(string message)
    {
        Lines.Add(message);
    }

    public void Error(System.Exception error)
    {
        Lines.Add(error?.ToString() ?? "unknown error");
    }
}
