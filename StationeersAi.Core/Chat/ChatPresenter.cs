using System;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;
using Caev;

namespace StationeersAi.Ai;

/// <summary>
/// Presents an assistant turn to chat: echoes the player's question, decides
/// which of the assistant's activity to show, and posts the answer. Chat writes
/// go through <see cref="IChatSink"/>, so this flow is testable without Unity.
/// </summary>
/// <remarks>
/// The presenter implements <see cref="IAssistantActivity"/>: reasoning, tool
/// calls and tool output are shown only when their option is enabled.
/// </remarks>
public sealed class ChatPresenter : IAssistantActivity
{
    private readonly IChatSink _chat;
    private readonly Func<IAssistantActivity, AiAssistant> _assistantFactory;
    private readonly Func<string> _playerName;
    private readonly string _assistantName;
    private readonly Logger _log;
    private readonly bool _showThinking;
    private readonly bool _showToolCalls;
    private readonly bool _showToolResults;

    /// <param name="chat">Destination for chat lines.</param>
    /// <param name="assistantFactory">
    /// Builds the assistant, or returns null when game data is not loaded. It
    /// receives the activity sink to report reasoning and tool use to.
    /// </param>
    /// <param name="playerName">Supplies the local player's name for the echo. May return null, which echoes as "You".</param>
    /// <param name="assistantName">Name the assistant posts under. May be null, which posts as "Sage".</param>
    /// <param name="log">Receives exception details.</param>
    /// <param name="showThinking">Post the model's reasoning and narration.</param>
    /// <param name="showToolCalls">Post each requested tool call.</param>
    /// <param name="showToolResults">Post each tool's raw output.</param>
    public ChatPresenter(
        IChatSink chat,
        Func<IAssistantActivity, AiAssistant> assistantFactory,
        Func<string> playerName,
        string assistantName,
        Logger log,
        bool showThinking,
        bool showToolCalls,
        bool showToolResults)
    {
        _chat = chat;
        _assistantFactory = assistantFactory;
        _playerName = playerName;
        _assistantName = assistantName;
        _log = log;
        _showThinking = showThinking;
        _showToolCalls = showToolCalls;
        _showToolResults = showToolResults;
    }

    /// <summary>Answers a question, echoing it first so it stays in the log.</summary>
    public async Task AskAsync(
        string prompt,
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Echo(prompt);

            var command = new RunTurnCommand(_assistantFactory, _log);
            command.Answered += Say;
            await command.ExecuteAsync(new RunTurnRequest(prompt), conversation, this, cancellationToken);
        }
        catch (AiClientException ex)
        {
            Say($"error: {ex.Message}");
            _log.Log(ex.ToString());
        }
        catch (Exception ex)
        {
            Say($"error: {ex.Message}");
            _log.Log(ex.ToString());
        }
    }

    /// <summary>Drops the conversation and reports how many messages remain.</summary>
    public void Clear(Conversation conversation)
    {
        conversation.Clear();
        Say($"context cleared ({conversation.Messages.Count} messages).");
    }

    /// <summary>Condenses the conversation into a single summary message.</summary>
    public async Task CompactAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        try
        {
            var assistant = _assistantFactory?.Invoke(this);
            if (assistant == null)
            {
                Say("game data is not loaded yet.");
                return;
            }

            if (conversation.Messages.Count == 0)
            {
                Say("nothing to compact.");
                return;
            }

            Say("compacting context...");
            var summary = await assistant.CompactAsync(conversation, cancellationToken);
            Say(string.IsNullOrWhiteSpace(summary)
                ? "compaction produced no summary."
                : "context compacted.");
        }
        catch (Exception ex)
        {
            Say($"error: {ex.Message}");
            _log.Log(ex.ToString());
        }
    }

    /// <summary>Posts an assistant line, prefixed with the assistant's name.</summary>
    public void Say(string text)
    {
        Write(ChatNaming.Apply(_assistantName, text));
    }

    /// <inheritdoc/>
    public void OnReasoning(string text)
    {
        if (_showThinking)
        {
            Say(text);
        }
    }

    /// <inheritdoc/>
    public void OnToolCall(string text)
    {
        if (_showToolCalls)
        {
            Say(text);
        }
    }

    /// <inheritdoc/>
    public void OnToolOutput(string text)
    {
        if (_showToolResults)
        {
            Say(text);
        }
    }

    private void Echo(string prompt)
    {
        var name = _playerName?.Invoke();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "You";
        }

        Write($"{name}: {prompt}");
    }

    private void Write(string line)
    {
        _chat.Write(line);
    }
}
