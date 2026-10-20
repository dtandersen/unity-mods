using OdinsAi.Entity;

namespace OdinsAi.Command
{
    /// <summary>A recipe together with whether the player has unlocked it.</summary>
    public sealed record UnlockedRecipe(Recipe Recipe, bool Unlocked);

    /// <summary>An upgrade step together with whether the player has unlocked it.</summary>
    public sealed record UnlockedUpgrade(Recipe Recipe, UpgradeLevel Upgrade, bool Unlocked);
}