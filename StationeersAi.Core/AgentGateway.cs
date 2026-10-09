using System.Threading;
using System.Threading.Tasks;
using Caev;

namespace StationeersAi.Core;

/// <summary>
/// Asking the agent to work, and condensing the history it keeps. This is the
/// port Core uses: the turn runner, the window, and the chat all ask through it
/// and listen on the <see cref="AgentEvents"/> the agent was built with.
/// </summary>
public interface AgentGateway
{
    /// <summary>
    /// Runs <paramref name="turn"/> and reports the answer through the events the
    /// agent was built with.
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
