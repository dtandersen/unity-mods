using Newtonsoft.Json;

namespace Sage.Entity;

/// <summary>
/// A stack of items carried by the player. <see cref="Quantity"/> is the
/// game's own quantity text (e.g. <c>x5</c>, <c>75%</c>), or null when the
/// item has no meaningful quantity. <see cref="Hand"/> is "Left" or
/// "Right" when the stack sits in a hand, else null.
/// </summary>
public sealed record ItemStack(
    string PrefabName,
    string? Name,
    string? Quantity,
    string Slot,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Hand = null);
