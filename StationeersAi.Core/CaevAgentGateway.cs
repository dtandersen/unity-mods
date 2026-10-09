using System.Threading;
using System.Threading.Tasks;
using Caev;
using Caev.Tools;

namespace StationeersAi.Core;

/// <summary>
/// The engine's agent behind Core's port: builds a <see cref="Agent"/> from the
/// model client, the tools, and the turn's events, then hands it the whole turn.
/// The turn says how to run — prompt text and iteration limit come in with it —
/// so one gateway answers any number of turns.
/// </summary>
public sealed class CaevAgentGateway : AgentGateway
{
    private readonly Agent _agent;

    public CaevAgentGateway(
        IAiClient client,
        ToolHandler tools,
        Logger log,
        AgentEvents? events)
    {
        _agent = new Agent(client, tools, log, events);
    }

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
