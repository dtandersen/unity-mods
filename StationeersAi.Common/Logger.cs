namespace StationeersAi.Core;

/// <summary>
/// Where the core writes its trace (questions, tool calls, answers). The
/// plugin implements this with the BepInEx log; tests record it instead.
/// </summary>
public interface Logger
{
    void Log(string message);
}
