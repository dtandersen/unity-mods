namespace OdinsAi.Tool
{
    /// <summary>The player's location as presented to the model.</summary>
    public sealed record LocationResult(float X, float Y, float Z, string Biome);
}