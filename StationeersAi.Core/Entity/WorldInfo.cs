using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>One gas in the outside air, as a share of the mix.</summary>
public sealed record GasMixEntry(string Gas, double Percent);

/// <summary>
/// The world: setup (name, gravity), orbit (day length, year, shape),
/// sunlight, and the current outside air. A live reading is null when the
/// game does not expose it.
/// </summary>
public sealed record WorldInfo(
    string? WorldName,
    string? WorldId,
    float Gravity,
    uint DaysPast,
    bool HasLava,
    bool IsCreative,
    string? DayLength,
    string? YearLength,
    double DaysPerYear,
    float Eccentricity,
    double SemiMajorAxisAu,
    float Inclination,
    bool TidallyLocked,
    float SolarIrradiance,
    float EarthSolarRatio,
    float TimeOfDay,
    bool IsEclipse,
    double? OutsideTemperatureC,
    double? OutsidePressureKpa,
    IReadOnlyList<GasMixEntry>? OutsideAir);
