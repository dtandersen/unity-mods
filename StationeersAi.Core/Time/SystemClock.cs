using System;
using System.Threading;
using System.Threading.Tasks;

namespace StationeersAi.Time
{
    /// <summary>The real system clock.</summary>
    public sealed class SystemClock : IClock
    {
        public static SystemClock Instance { get; } = new SystemClock();

        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public Task Delay(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            return Task.Delay(duration, cancellationToken);
        }
    }
}
