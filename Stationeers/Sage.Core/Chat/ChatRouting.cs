using System;
using Caev;

namespace Sage.Ai;

/// <summary>
/// One chat line as the game displayed it: who posted it, and what it says.
/// </summary>
/// <param name="DisplayName">Name the game showed. Null or blank is nobody's name.</param>
/// <param name="Text">The message text.</param>
public sealed record ChatLine(string? DisplayName, string? Text);

/// <summary>
/// What routing is allowed to ask about: the agent's name, the chat trigger,
/// whether the stationeer wants mentions answered, and whether anyone else is in
/// the game. The mod reads all four, so routing stays free of Unity and config.
/// </summary>
/// <param name="AgentName">Name the agent posts under.</param>
/// <param name="MentionTrigger">Chat prefix that addresses the agent.</param>
/// <param name="RespondToMentions">Answering is on. When off, nothing is a question.</param>
/// <param name="OthersInGame">Another player is in the game, so only a mention addresses the agent.</param>
public sealed record ChatRoutingRules(
    string AgentName,
    string MentionTrigger,
    bool RespondToMentions,
    bool OthersInGame);

/// <summary>
/// What to do with one chat line.
/// </summary>
/// <param name="FromAgent">The agent posted this line.</param>
/// <param name="ShouldAnswer">Ask the agent this question.</param>
/// <param name="Prompt">The question, trigger removed. Null when nothing should be answered.</param>
/// <param name="Mentioned">The line started with the trigger, rather than being addressed by being alone.</param>
/// <param name="Considered">The line was worth noting in the log: not the agent's own, answering is on, and it says something.
/// A line nobody could read is not one, so a switched-off mod stays quiet instead of copying all chat.</param>
public sealed record ChatDecision(
    bool FromAgent,
    bool ShouldAnswer,
    string? Prompt,
    bool Mentioned,
    bool Considered);

/// <summary>
/// Decides whether a displayed chat line is a question for the agent.
/// </summary>
/// <remarks>
/// The agent's own lines are recognised first and never answered: on a server the
/// answer comes back through the display path like anyone else's, and answering it
/// would ask forever. With nobody else in the game every line is addressed to the
/// agent, so an unmentioned line is answered as written; with another player only a
/// mention is. Nothing here decides what the Sage window shows: the presenter
/// listens on the gateway's channel and writes the window itself.
/// </remarks>
public static class ChatRouting
{
    public static ChatDecision Decide(ChatLine line, ChatRoutingRules rules)
    {
        var text = line.Text ?? string.Empty;
        var fromAgent = IsAgentLine(line.DisplayName, rules.AgentName);
        var considered = !fromAgent && rules.RespondToMentions && !string.IsNullOrWhiteSpace(text);

        if (!considered)
        {
            return new ChatDecision(fromAgent, false, null, false, false);
        }

        if (Mention.TryParse(rules.MentionTrigger, text, out var prompt))
        {
            return new ChatDecision(false, true, prompt, true, true);
        }

        return rules.OthersInGame
            ? new ChatDecision(false, false, null, false, true)
            : new ChatDecision(false, true, text.Trim(), false, true);
    }

    private static bool IsAgentLine(string? displayName, string? agentName)
    {
        return !string.IsNullOrWhiteSpace(displayName)
            && !string.IsNullOrWhiteSpace(agentName)
            && displayName!.Equals(agentName, StringComparison.OrdinalIgnoreCase);
    }
}
