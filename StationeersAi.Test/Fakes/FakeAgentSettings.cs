using StationeersAi.Core;

namespace StationeersAi.Test.Fakes;

/// <summary>
/// Turn settings for specs: instructions and an iteration limit, both blank by
/// default, so a spec says only what the turn is about.
/// </summary>
public sealed class FakeAgentSettings : AgentSettings
{
    public FakeAgentSettings(string systemPrompt = null, int maxIterations = 5)
    {
        SystemPrompt = systemPrompt;
        MaxIterations = maxIterations;
    }

    /// <summary>Instructions sent ahead of the conversation. Null or blank sends none.</summary>
    public string SystemPrompt { get; }

    /// <summary>Tool round-trips allowed per question.</summary>
    public int MaxIterations { get; }
}
