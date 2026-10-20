namespace Crowbus.Common;

/// <summary>
/// Where the core writes its trace (questions, tool calls, answers). The
/// plugin implements this with the BepInEx log; tests record it instead.
/// </summary>
public interface Logger
{
    void Info(string message);

    void Warn(string message);

    void Error(System.Exception error);
}
