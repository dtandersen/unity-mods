using System;
using System.Threading;
using System.Threading.Tasks;
using Caev;
using StationeersAi.Core;
using Caev.Tools;

namespace StationeersAi.Command;

/// <summary>One agent turn: the prompt to run.</summary>
public sealed class RunTurnRequest
{
    public RunTurnRequest(string prompt)
    {
        Prompt = prompt ?? string.Empty;
    }

    public string Prompt { get; }
}

/// <summary>
/// Runs one agent turn and reports it through <see cref="AgentEvents"/>:
/// reasoning, tool calls and tool output as they happen, and the answer at the
/// end. The agent reports its own answer; this only reports what never
/// reaches it — the game-not-loaded notice and errors — as answers too, so the
/// turn itself never throws. The caller owns the events object and subscribes
/// to what it shows.
/// </summary>
public sealed class RunTurnCommand
{
    private readonly Func<AgentEvents, AgentGateway?> _gatewayFactory;
    private readonly Logger _log;

    /// <param name="gatewayFactory">
    /// Builds the agent, or returns null when game data is not loaded. It
    /// receives the events the turn reports through.
    /// </param>
    /// <param name="log">Receives exception details.</param>
    public RunTurnCommand(Func<AgentEvents, AgentGateway?> gatewayFactory, Logger log)
    {
        _gatewayFactory = gatewayFactory;
        _log = log;
    }

    /// <param name="request">The turn to run.</param>
    /// <param name="conversation">History sent with the request. May be null.</param>
    /// <param name="events">
    /// Where the turn reports: reasoning, tool traffic, then the answer. Never null.
    /// </param>
    public async Task ExecuteAsync(
        RunTurnRequest request,
        Conversation? conversation,
        AgentEvents events,
        CancellationToken cancellationToken = default)
    {
        var prompt = request != null ? request.Prompt : string.Empty;
        try
        {
            var agent = _gatewayFactory?.Invoke(events);
            if (agent == null)
            {
                events.RaiseAnswer("game data is not loaded yet.");
                return;
            }

            // The agent raises its own answer through the events it was built with.
            await agent.AskAsync(prompt, conversation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            events.RaiseAnswer($"error: {ex.Message}");
            _log.Info(ex.ToString());
        }
    }
}
