using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OdinsAi.Time;

namespace OdinsAi.Test.Fakes
{
    /// <summary>
    /// A clock the test drives by hand. <see cref="Delay"/> returns a task that only
    /// completes once <see cref="Advance"/> moves past its due time.
    /// </summary>
    public sealed class FakeClock : IClock
    {
        private readonly object _sync = new object();
        private readonly List<Waiter> _waiters = new List<Waiter>();

        public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UnixEpoch;

        public Task Delay(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_sync)
            {
                _waiters.Add(new Waiter(UtcNow + duration, completion));
            }

            return completion.Task;
        }

        /// <summary>Moves time forward, completing any delays that are now due.</summary>
        public void Advance(TimeSpan delta)
        {
            List<Waiter> due;
            lock (_sync)
            {
                UtcNow += delta;
                due = _waiters.FindAll(waiter => waiter.Due <= UtcNow);
                _waiters.RemoveAll(waiter => waiter.Due <= UtcNow);
            }

            foreach (var waiter in due)
            {
                waiter.Completion.TrySetResult(true);
            }
        }

        private sealed class Waiter
        {
            public Waiter(DateTimeOffset due, TaskCompletionSource<bool> completion)
            {
                Due = due;
                Completion = completion;
            }

            public DateTimeOffset Due { get; }

            public TaskCompletionSource<bool> Completion { get; }
        }
    }
}