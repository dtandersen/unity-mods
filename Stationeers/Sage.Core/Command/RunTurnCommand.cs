using System;
using System.Threading;
using System.Threading.Tasks;
using Caev;
using Sage.Core;
using Caev.Tools;

namespace Sage.Command;

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
/// Runs one agent turn. The gateway reports reasoning, tool traffic and the
/// answer on its own channel; this adds an answer only for what never reaches the
/// agent — an error — so a turn always ends with exactly one answer on that
/// channel.
/// </summary>
public sealed class RunTurnCommand
{
    private readonly AgentGateway _gateway;
    private readonly AgentSettings _settings;
    private readonly Logger _log;

    /// <param name="gateway">The agent to ask, and the channel it reports to.</param>
    /// <param name="settings">
    /// Instructions and iteration limit for the turn, read from the game's config.
    /// </param>
    /// <param name="log">Receives exception details.</param>
    public RunTurnCommand(AgentGateway gateway, AgentSettings settings, Logger log)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _settings = settings;
        _log = log;
    }

    /// <param name="request">The turn to run.</param>
    /// <param name="conversation">History sent with the request. May be null.</param>
    public async Task ExecuteAsync(
        RunTurnRequest request,
        Conversation? conversation,
        CancellationToken cancellationToken = default)
    {
        var prompt = request != null ? request.Prompt : string.Empty;
        try
        {
            await _gateway.AskAsync(
                new TurnContext(prompt, conversation, _settings.SystemPrompt, _settings.MaxIterations),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _gateway.Events.RaiseAnswer($"error: {ex.Message}");
            _log.Info(ex.ToString());
        }
    }
}
