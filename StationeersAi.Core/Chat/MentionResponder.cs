using System;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Command;
using StationeersAi.Core;
using Caev;

namespace StationeersAi.Ai;

/// <summary>
/// Answers public chat mentions. Only one question is handled at a time: while
/// an answer is being produced, further mentions are ignored. The answer itself
/// reaches chat through <see cref="ChatAnswerSink"/>, which listens on the
/// gateway, so this holds no sink of its own.
/// </summary>
public sealed class MentionResponder
{
    private readonly RunTurnCommand _runTurn;
    private bool _busy;

    /// <param name="gateway">The agent to ask, and the channel it reports to.</param>
    /// <param name="settings">Instructions and iteration limit for each turn.</param>
    /// <param name="log">Receives exception details.</param>
    public MentionResponder(AgentGateway gateway, AgentSettings settings, Logger log)
    {
        _runTurn = new RunTurnCommand(gateway, settings, log);
    }

    /// <summary>Whether a question is currently being answered.</summary>
    public bool IsBusy => _busy;

    /// <summary>
    /// Answers <paramref name="prompt"/>, sending <paramref name="conversation"/>
    /// history with the request (null sends none). Returns false (and does
    /// nothing) when a question is already being answered.
    /// </summary>
    public async Task<bool> HandleAsync(string prompt, Conversation? conversation, CancellationToken cancellationToken = default)
    {
        if (_busy)
        {
            return false;
        }

        _busy = true;
        try
        {
            await _runTurn.ExecuteAsync(new RunTurnRequest(prompt), conversation, cancellationToken);
            return true;
        }
        finally
        {
            _busy = false;
        }
    }
}
