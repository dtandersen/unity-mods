using StationeersAi.Core;
using System;
using System.Threading;

namespace StationeersAi.Game;

/// <summary>
/// Runs work on Unity's main thread. Built once in <c>Awake</c> from the
/// captured synchronization context, then handed to game-data adapters
/// through constructors.
/// </summary>
public sealed class MainThreadExecutor : IMainThread
{
    private readonly SynchronizationContext? _synchronizationContext;
    private readonly int _unityThreadId;
    private readonly Logger _log;

    public MainThreadExecutor(SynchronizationContext? synchronizationContext, int unityThreadId, Logger log)
    {
        _synchronizationContext = synchronizationContext;
        _unityThreadId = unityThreadId;
        _log = log;
    }

    /// <summary>Runs work on Unity's main thread, posting from async continuations.</summary>
    public void Run(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
        {
            RunGuarded(action);
            return;
        }

        var synchronizationContext = _synchronizationContext;
        if (synchronizationContext == null)
        {
            _log.Log("cannot dispatch to Unity main thread: synchronization context was not captured");
            return;
        }

        synchronizationContext.Post(_ => RunGuarded(action), null);
    }

    /// <summary>
    /// Runs work on Unity's main thread and returns its result, blocking the
    /// caller. Tools run off the main thread, but Unity objects demand it.
    /// Throws when the context was never captured or the wait times out.
    /// </summary>
    public T RunSync<T>(Func<T> func, int timeoutMilliseconds = 10000)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
        {
            return func();
        }

        var synchronizationContext = _synchronizationContext;
        if (synchronizationContext == null)
        {
            throw new InvalidOperationException("Unity synchronization context was not captured.");
        }

        T result = default!;
        Exception? error = null;
        using (var done = new ManualResetEventSlim(false))
        {
            synchronizationContext.Post(_ =>
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done.Set();
                }
            }, null);

            if (!done.Wait(timeoutMilliseconds))
            {
                throw new TimeoutException("Timed out waiting for Unity's main thread.");
            }
        }

        if (error != null)
        {
            throw error;
        }

        return result;
    }

    private void RunGuarded(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
        }
    }
}
