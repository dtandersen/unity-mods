using BepInEx.Logging;
using Logger = Sage.Core.Logger;

namespace Sage.Mod;

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
        _logger.LogInfo(Sage.Formatting.LogText.Sanitize(message));
    }

    public void Warn(string message)
    {
        _logger.LogWarning(Sage.Formatting.LogText.Sanitize(message));
    }

    public void Error(System.Exception error)
    {
        _logger.LogError(error?.ToString() ?? "unknown error");
        Flush();
    }

    /// <summary>
    /// Pushes held log lines to disk. BepInEx holds disk writes back
    /// about 2 seconds by default, so call this after errors and unload
    /// to keep crash context in LogOutput.log.
    /// </summary>
    public void Flush()
    {
        foreach (var listener in BepInEx.Logging.Logger.Listeners)
        {
            if (listener is DiskLogListener disk && disk.LogWriter != null)
            {
                try
                {
                    disk.LogWriter.Flush();
                }
                catch (System.Exception)
                {
                    // Never let log flushing break the game.
                }
            }
        }
    }
}
