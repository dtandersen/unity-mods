using System;
using System.Collections.Generic;
using Assets.Scripts;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Reads the world, orbit, sunlight, and outside air from game statics.
    /// Snapshotted on the calling (main) thread. Tolerates a missing world
    /// (main menu, loading): the info is then null, and single missing
    /// readings stay null rather than failing the whole snapshot.
    /// </summary>
    public sealed class StationeersWorldContext : IWorldContext
    {
        public StationeersWorldContext()
        {
            Info = Read();
        }

        public WorldInfo Info { get; }

        private static WorldInfo Read()
        {
            try
            {
                if (WorldSetting.Current == null)
                {
                    return null;
                }

                var body = Safe(OrbitalSimulation.GetPlayerBody);

                return new WorldInfo(
                    WorldName: Safe(() => WorldManager.CurrentWorldName),
                    WorldId: Safe(() => WorldManager.CurrentWorldId),
                    Gravity: Safe(() => WorldManager.WorldGravity),
                    DaysPast: Safe(() => WorldManager.DaysPast),
                    HasLava: Safe(() => WorldManager.HasLava),
                    IsCreative: Safe(() => WorldManager.IsCreative()),
                    DayLength: FormatSeconds(Safe(() => OrbitalSimulation.GetDayLengthSeconds())),
                    YearLength: body != null ? FormatTime(body.GetSiderealYearLength()) : null,
                    DaysPerYear: body != null ? Safe(() => body.DaysPerPlanetYear) : 0,
                    Eccentricity: body?.Orbit != null ? Safe(() => body.Orbit.Eccentricity) : 0,
                    SemiMajorAxisAu: body?.Orbit != null ? Safe(() => body.Orbit.SemiMajorAxis) : 0,
                    Inclination: body?.Orbit != null ? Safe(() => body.Orbit.Inclination) : 0,
                    TidallyLocked: body != null && Safe(() => body.TidallyLocked),
                    SolarIrradiance: Safe(() => OrbitalSimulation.SolarIrradiance),
                    EarthSolarRatio: Safe(() => OrbitalSimulation.EarthSolarRatio),
                    TimeOfDay: Safe(() => OrbitalSimulation.TimeOfDay),
                    IsEclipse: Safe(() => OrbitalSimulation.IsEclipse),
                    OutsideTemperatureC: Safe(() => PlanetaryAtmosphereSimulation.AggregateTemperature.ToCelsius()),
                    OutsidePressureKpa: Safe(() => (double)PlanetaryAtmosphereSimulation.GlobalPressure.ToFloat()),
                    OutsideAir: ReadAirMix());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static IReadOnlyList<GasMixEntry> ReadAirMix()
        {
            try
            {
                var mix = WorldSetting.Current.Data.GlobalAtmosphereData.GlobalGasMixData.GlobalMoleDatas;
                var total = 0f;
                foreach (var mole in mix)
                {
                    total += mole.Quantity;
                }

                if (total <= 0)
                {
                    return new GasMixEntry[0];
                }

                var entries = new List<GasMixEntry>();
                foreach (var mole in mix)
                {
                    if (mole.Quantity > 0)
                    {
                        entries.Add(new GasMixEntry(mole.Type.ToString(), mole.Quantity / total * 100));
                    }
                }

                entries.Sort((a, b) => b.Percent.CompareTo(a.Percent));
                return entries;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string FormatSeconds(int seconds)
        {
            if (seconds <= 0)
            {
                return null;
            }

            if (seconds >= 3600)
            {
                return $"{seconds / 3600}h {seconds % 3600 / 60}m";
            }

            if (seconds >= 60)
            {
                return $"{seconds / 60}m";
            }

            return $"{seconds}s";
        }

        private static string FormatTime(TimeLength length)
        {
            try
            {
                if (length.Days > 0)
                {
                    return length.Hours > 0 ? $"{length.Days}d {length.Hours}h" : $"{length.Days}d";
                }

                if (length.Hours > 0)
                {
                    return length.Minutes > 0 ? $"{length.Hours}h {length.Minutes}m" : $"{length.Hours}h";
                }

                return FormatSeconds((int)(length.Minutes * 60 + length.Seconds));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static T Safe<T>(Func<T> read)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
