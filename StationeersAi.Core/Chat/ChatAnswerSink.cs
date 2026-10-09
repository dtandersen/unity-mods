using System;
using Caev;
using StationeersAi.Core;
using StationeersAi.Formatting;

namespace StationeersAi.Ai;

/// <summary>
/// Posts every answer the gateway reports to chat, in plain text, under the
/// agent's name. One subscriber for the life of the gateway: the agent always
/// answers in chat, so this is where an answer becomes a public line.
/// </summary>
public sealed class ChatAnswerSink : IDisposable
{
    private readonly IChatSink _chat;
    private readonly string _agentName;
    private readonly AgentEvents _events;

    public ChatAnswerSink(AgentGateway gateway, IChatSink chat, string agentName)
    {
        if (gateway == null)
        {
            throw new ArgumentNullException(nameof(gateway));
        }

        _chat = chat ?? throw new ArgumentNullException(nameof(chat));
        _agentName = agentName;
        _events = gateway.Events;
        _events.OnAnswer += OnAnswer;
    }

    /// <summary>Stops posting answers, for a gateway being thrown away.</summary>
    public void Dispose()
    {
        _events.OnAnswer -= OnAnswer;
    }

    private void OnAnswer(string text)
    {
        _chat.Write(ChatNaming.Apply(_agentName, MarkdownToPlain.ToPlainText(text)));
    }
}
