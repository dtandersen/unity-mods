using System.Threading;
using System.Threading.Tasks;
using Caev;

namespace StationeersAi.Core;

/// <summary>
/// Asking the agent to work, and condensing the history it keeps. This is the
/// port Core uses: the turn runner, the window, and the chat all ask through it
/// and listen on <see cref="Events"/>, the one channel the agent reports to.
/// </summary>
public interface AgentGateway
{
    /// <summary>
    /// What the agent reports to: reasoning, tool calls, tool output, and the
    /// answer. One channel for the life of the gateway, so listeners subscribe once.
    /// </summary>
    AgentEvents Events { get; }

    /// <summary>
    /// Runs <paramref name="turn"/> and reports the answer through <see cref="Events"/>.
    /// </summary>
    Task AskAsync(TurnContext turn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Condenses <paramref name="conversation"/> into one summary message and returns
    /// it, or returns null when there is nothing to condense or the model gave no summary.
    /// </summary>
    Task<string?> CompactAsync(
        Conversation? conversation,
        CancellationToken cancellationToken = default);
}
