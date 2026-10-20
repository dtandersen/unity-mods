namespace OdinsAi.Entity
{
    /// <summary>A world boss and whether the player has slain it.</summary>
    public sealed record BossStatus(string Name, bool Defeated);
}