namespace StationeersAi.Entity;

/// <summary>
/// An item from the game (a <c>DynamicThing</c> prefab). <see cref="Type"/> is
/// a coarse category such as the item's class name (e.g. <c>Ingot</c>).
/// </summary>
public sealed record Item(string PrefabName, string Name, string Type);
