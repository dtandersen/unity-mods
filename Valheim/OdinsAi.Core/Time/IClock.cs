using System;
using System.Threading;
using System.Threading.Tasks;

namespace OdinsAi.Time
{
    /// <summary>
    /// Time source for the assistant, so delays and elapsed-silence checks can be
    /// faked in tests instead of waiting on the wall clock.
    /// </summary>
    public interface IClock
    {
        /// <summary>The current instant.</summary>
        DateTimeOffset UtcNow { get; }

        /// <summary>Completes after <paramref name="duration"/> has elapsed.</summary>
        Task Delay(TimeSpan duration, CancellationToken cancellationToken = default);
    }
}