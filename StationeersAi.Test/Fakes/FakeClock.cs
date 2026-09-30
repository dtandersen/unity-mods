using System;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Time;

namespace StationeersAi.Test.Fakes
{
    /// <summary>Manual clock for tests: time only advances when told to.</summary>
    public sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public void Advance(TimeSpan duration)
        {
            UtcNow += duration;
        }

        public Task Delay(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            // Tests never wait: a delay completes immediately (time is advanced
            // explicitly via Advance when elapsed-time behaviour is exercised).
            return Task.CompletedTask;
        }
    }
}
