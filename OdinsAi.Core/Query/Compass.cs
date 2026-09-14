using System;

namespace OdinsAi.Query
{
    /// <summary>Maps a world-space direction to an 8-way compass point.</summary>
    /// <remarks>Valheim convention: +Z is north and +X is east.</remarks>
    public static class Compass
    {
        private static readonly string[] Points = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        /// <summary>Compass point for the vector (<paramref name="dx"/>, <paramref name="dz"/>).</summary>
        public static string Direction(float dx, float dz)
        {
            var degrees = Math.Atan2(dx, dz) * (180.0 / Math.PI);
            var normalized = ((degrees % 360.0) + 360.0) % 360.0;
            var index = (int)Math.Round(normalized / 45.0, MidpointRounding.AwayFromZero) % 8;
            return Points[index];
        }
    }
}