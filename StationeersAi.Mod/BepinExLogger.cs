using BepInEx.Logging;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Mod;

/// <summary>
/// The BepInEx implementation of <see cref="Core.Logger"/>: forwards core trace
/// lines to the plugin log.
/// </summary>
public sealed class BepinExLogger : Logger
{
    private readonly ManualLogSource _logger;

    public BepinExLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    public void Info(string message)
    {
        _logger.LogInfo(StationeersAi.Formatting.LogText.Sanitize(message));
    }

    public void Warn(string message)
    {
        _logger.LogWarning(StationeersAi.Formatting.LogText.Sanitize(message));
    }

    public void Error(System.Exception error)
    {
        _logger.LogError(error?.ToString() ?? "unknown error");
    }
}
