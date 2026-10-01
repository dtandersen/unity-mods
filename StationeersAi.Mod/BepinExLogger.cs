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

    public void Log(string message)
    {
        _logger.LogInfo(StationeersAi.Formatting.LogText.Sanitize(message));
    }
}
