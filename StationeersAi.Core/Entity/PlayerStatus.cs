namespace StationeersAi.Entity
{
    /// <summary>
    /// The player's status: where they are and the world they are in.
    /// <see cref="Details"/> carries a short human-readable summary (suit,
    /// health, hunger, atmosphere) when the game exposes it, or null.
    /// </summary>
    public sealed record PlayerStatus(float X, float Y, float Z, string World, string Details);
}
