using System;

namespace StationeersAi.Game;

/// <summary>
/// Runs work on Unity's main thread. The plugin implements this and hands it
/// to game-data adapters through constructors, so only the plugin touches
/// Unity threading.
/// </summary>
public interface IMainThread
{
    /// <summary>Runs an action on the main thread, posting from elsewhere.</summary>
    void Run(Action action);

    /// <summary>
    /// Runs a function on the main thread and returns its result, blocking
    /// the caller. Throws when the wait times out.
    /// </summary>
    T RunSync<T>(Func<T> func, int timeoutMilliseconds = 10000);
}
