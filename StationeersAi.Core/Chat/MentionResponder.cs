using System;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;
using Caev;

namespace StationeersAi.Ai;

/// <summary>
/// Answers public chat mentions. Only one question is handled at a time: while
/// an answer is being produced, further mentions are ignored. The answer is
/// broadcast as plain text under the assistant's name.
/// </summary>
public sealed class MentionResponder
{
    private readonly IChatSink _chat;
    private readonly Func<IAssistantActivity, AiAssistant> _assistantFactory;
    private readonly string _assistantName;
    private readonly Logger _log;
    private bool _busy;

    /// <param name="chat">Destination for the answer.</param>
    /// <param name="assistantFactory">
    /// Builds the assistant, or returns null when game data is not loaded. It
    /// receives the activity sink to report reasoning and tool use to. Called
    /// on the calling thread (the Unity main thread) before the first await.
    /// </param>
    /// <param name="assistantName">Name the answer is posted under.</param>
    /// <param name="log">Receives exception details.</param>
    public MentionResponder(
        IChatSink chat,
        Func<IAssistantActivity, AiAssistant> assistantFactory,
        string assistantName,
        Logger log)
    {
        _chat = chat ?? throw new ArgumentNullException(nameof(chat));
        _assistantFactory = assistantFactory ?? throw new ArgumentNullException(nameof(assistantFactory));
        _assistantName = assistantName;
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>Whether a question is currently being answered.</summary>
    public bool IsBusy => _busy;

    /// <summary>
    /// Answers <paramref name="prompt"/>, sending <paramref name="conversation"/>
    /// history with the request (null sends none). Returns false (and does
    /// nothing) when a question is already being answered.
    /// </summary>
    public async Task<bool> HandleAsync(string prompt, Conversation conversation, CancellationToken cancellationToken = default)
    {
        if (_busy)
        {
            return false;
        }

        _busy = true;
        try
        {
            var command = new RunTurnCommand(_assistantFactory, _log);
            command.Answered += Say;
            await command.ExecuteAsync(new RunTurnRequest(prompt), conversation, null, cancellationToken);
            return true;
        }
        finally
        {
            _busy = false;
        }
    }

    private void Say(string text)
    {
        _chat.Write(ChatNaming.Apply(_assistantName, text));
    }
}
