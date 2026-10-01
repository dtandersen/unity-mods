using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Time;

namespace StationeersAi.Test.Fakes;

/// <summary>
/// Manual clock for tests: time only advances when told to. Pending delays
/// complete when the clock passes their due time, so presenter loops yield
/// instead of spinning.
/// </summary>
public sealed class FakeClock : IClock
{
    private readonly List<(DateTimeOffset Due, TaskCompletionSource<bool> Gate)> _pending =
        new List<(DateTimeOffset, TaskCompletionSource<bool>)>();

    private readonly object _gate = new object();

    public DateTimeOffset UtcNow { get; set; } = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan duration)
    {
        List<TaskCompletionSource<bool>> due = new List<TaskCompletionSource<bool>>();
        lock (_gate)
        {
            UtcNow += duration;
            for (var i = _pending.Count - 1; i >= 0; i--)
            {
                if (_pending[i].Due <= UtcNow)
                {
                    due.Add(_pending[i].Gate);
                    _pending.RemoveAt(i);
                }
            }
        }

        foreach (var gate in due)
        {
            gate.TrySetResult(true);
        }
    }

    public Task Delay(TimeSpan duration, CancellationToken cancellationToken = default)
    {
        if (duration <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            _pending.Add((UtcNow + duration, gate));
        }

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => gate.TrySetCanceled(cancellationToken));
        }

        return gate.Task;
    }
}
