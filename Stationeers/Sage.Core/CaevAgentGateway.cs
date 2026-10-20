using System.Threading;
using System.Threading.Tasks;
using Caev;
using Caev.Tools;

namespace Sage.Core;

/// <summary>
/// The engine's agent behind Core's port: builds a <see cref="Agent"/> from the
/// model client, the tools, and the channel it reports to, then hands it the whole
/// turn. The turn says how to run — prompt text and iteration limit come in with it
/// — so one gateway answers any number of turns, and listeners subscribe once.
/// </summary>
public sealed class CaevAgentGateway : AgentGateway
{
    private readonly Agent _agent;

    public CaevAgentGateway(
        IAiClient client,
        ToolHandler tools,
        Logger log,
        AgentEvents? events = null)
    {
        Events = events ?? new AgentEvents();
        _agent = new Agent(client, tools, log, Events);
    }

    /// <summary>The one channel this gateway's agent reports to, for the life of the gateway.</summary>
    public AgentEvents Events { get; }

    public Task AskAsync(TurnContext turn, CancellationToken cancellationToken = default)
    {
        return _agent.AskAsync(turn, cancellationToken);
    }

    public Task<string?> CompactAsync(
        Conversation? conversation,
        CancellationToken cancellationToken = default)
    {
        return _agent.CompactAsync(conversation, cancellationToken);
    }
}
