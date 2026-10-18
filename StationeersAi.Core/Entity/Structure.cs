using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>
/// A buildable structure from the game (a <c>Structure</c> prefab).
/// <see cref="Description"/> is the in-game tooltip, when available.
/// <see cref="Steps"/> is how it builds, one step per build state in order
/// (e.g. the furnace takes a kit, then wrench-held sheets, then welding).
/// </summary>
public sealed record Structure(string PrefabName, string Name, string? Description, IReadOnlyList<BuildStep> BuildStates);
