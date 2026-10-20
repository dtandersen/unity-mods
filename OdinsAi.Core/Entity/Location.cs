namespace OdinsAi.Entity
{
    /// <summary>The player's current position and the biome they are standing in.</summary>
    public sealed record Location(float X, float Y, float Z, string Biome);
}