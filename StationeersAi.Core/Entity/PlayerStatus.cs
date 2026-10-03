namespace StationeersAi.Entity;

/// <summary>
/// The player's status: where they are, the world they are in, the
/// room they stand in (null outside rooms), and the compass heading they
/// face in degrees (0 north, 90 east; null when unknown). <see cref="Details"/>
/// carries a short human-readable summary (suit, health, hunger, atmosphere)
/// when the game exposes it, or null.
/// </summary>
public sealed record PlayerStatus(float X, float Y, float Z, string World, string Details, long? RoomId, double? FacingYaw);
