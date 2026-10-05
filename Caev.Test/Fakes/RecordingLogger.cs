using System.Collections.Generic;
using StationeersAi.Core;

namespace Caev.Test.Fakes;

/// <summary>Records trace lines for assertions.</summary>
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
