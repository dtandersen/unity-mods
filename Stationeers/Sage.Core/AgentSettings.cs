namespace Sage.Core;

/// <summary>
/// What the game's config says about how a turn runs. The plugin reads the
/// BepInEx entries, so the values change when the stationeer edits the config —
/// Core asks per turn and never caches an answer.
/// </summary>
public interface AgentSettings
{
    /// <summary>Instructions sent ahead of the conversation. May be null or blank.</summary>
    string? SystemPrompt { get; }

    /// <summary>Tool round-trips allowed before the agent gives up.</summary>
    int MaxIterations { get; }
}
