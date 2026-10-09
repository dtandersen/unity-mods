using System.Threading;
using System.Threading.Tasks;

namespace Caev;

/// <summary>
/// One question answered by an agent. Implemented by <see cref="Agent"/> and
/// by a fake in specs that only need to know what a turn reports.
/// </summary>
public interface AgentGateway
{
    /// <summary>
    /// Answers <paramref name="prompt"/> and reports the answer through the events
    /// it was built with.
    /// </summary>
    Task AskAsync(
        string prompt,
        Conversation? conversation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Condenses <paramref name="conversation"/> into one summary message and returns
    /// it, or returns null when there is nothing to condense or the model gave no summary.
    /// </summary>
    Task<string?> CompactAsync(
        Conversation? conversation,
        CancellationToken cancellationToken = default);
}
