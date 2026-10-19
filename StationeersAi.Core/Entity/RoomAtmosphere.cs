using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>One gas in a room, in moles.</summary>
public sealed record GasMoles(string MoleType, double Moles);

/// <summary>
/// A room's atmospherics: what kind of room it is, temperature, pressure,
/// volume, and the gas mix in moles. A live reading is null when the game
/// does not expose it.
/// </summary>
public sealed record RoomAtmosphere(
    long RoomId,
    string RoomType,
    double TemperatureC,
    double PressureKpa,
    double VolumeLitres,
    IReadOnlyList<GasMoles> Gases);
