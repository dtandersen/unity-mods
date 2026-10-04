namespace StationeersAi.Entity;

/// <summary>
/// A buildable structure from the game (a <c>Structure</c> prefab).
/// <see cref="Description"/> is the in-game tooltip, when available.
/// </summary>
public sealed record Structure(string Id, string Name, string? Description);
