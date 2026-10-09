using System.Threading;
using System.Threading.Tasks;
using Caev;
using Caev.Tools;

namespace StationeersAi.Core;

/// <summary>
/// The engine's agent behind Core's port: builds a <see cref="Agent"/> from the
/// model client, the tools, and the turn's events, then forwards the two calls
/// Core makes. The engine stays free of this port, and specs run the real loop
/// through the same gateway the mod uses.
/// </summary>
public sealed class CaevAgentGateway : AgentGateway
{
    private readonly Agent _agent;

    public CaevAgentGateway(
        IAiClient client,
        ToolHandler tools,
        string? systemPrompt,
        int maxIterations,
        Logger log,
        AgentEvents? events)
    {
        _agent = new Agent(client, tools, systemPrompt, maxIterations, log, events);
    }

    public Task AskAsync(
        string prompt,
        Conversation? conversation,
        CancellationToken cancellationToken = default)
    {
        return _agent.AskAsync(prompt, conversation, cancellationToken);
    }

    public Task<string?> CompactAsync(
        Conversation? conversation,
        CancellationToken cancellationToken = default)
    {
        return _agent.CompactAsync(conversation, cancellationToken);
    }
}
