using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>
/// One “used in” line of a Stationpedia page: what consumes the subject
/// (<see cref="Title"/>, e.g. <c>Generator (Solid Fuel)</c>), with detail
/// when the game states any (e.g. <c>100 kW Energy</c> for fuels), and the
/// key of the consumer's own page when one exists. When the subject is a
/// solid fuel and the consumer is a power generator, the numeric fields
/// carry the same maths the game uses: energy per second of one unit,
/// the generator output, burn ticks, total joules, burn seconds for one
/// unit, and the tick length. All numeric fields hold null when the game
/// gives none.
/// </summary>
public sealed record StationpediaUse(string Title, string? Detail, string? Key, double? EnergyPerSecondW = null, double? GeneratorPowerW = null, int? BurnTicks = null, double? TotalJoules = null, double? BurnSeconds = null, double? TickSeconds = null);

/// <summary>
/// One “how to manufacture” line of a Stationpedia page: the station that
/// makes the subject (<see cref="Station"/>, e.g. <c>Advanced Furnace</c>),
/// its tier when the game states one, the requirements text (reagents with
/// quantities plus smelt conditions such as temperature and pressure), and
/// the key of the station's own page when one exists.
/// </summary>
public sealed record StationpediaMake(string Station, string? Tier, string? Detail, string? Key);

/// <summary>
/// One logic line of a Stationpedia page: the logic variable name
/// (e.g. <c>Power</c>, <c>On</c>, <c>ForceWrite</c>) and how it can be used
/// (<c>Read</c>, <c>Write</c>, or <c>Read Write</c>).
/// </summary>
public sealed record StationpediaLogic(string Name, string Access);

/// <summary>
/// One connection of a Stationpedia page: the port name (e.g.
/// <c>Data Input</c>, <c>Data Output</c>) and its index on the prefab
/// (e.g. <c>0</c>, <c>1</c>). The game numbers ports in build order.
/// </summary>
public sealed record StationpediaConnection(string Name, string Index);

/// <summary>
/// One combustion output of a Stationpedia page: the gas made and how much.
/// Gas holds the game symbol (e.g. <c>CO2</c>, <c>X</c> for pollutant).
/// </summary>
public sealed record StationpediaCombustionOutput(string Gas, double Quantity);

/// <summary>
/// One combustion line of a Stationpedia page: what burns with what and what
/// it makes. Amounts are mole counts from the game tables. AutoIgnitionC and
/// OutputTemperatureC hold degrees Celsius, null when the game gives none.
/// </summary>
public sealed record StationpediaCombustion(string Fuel, string Oxidiser, double FuelAmount, double OxidiserAmount, IReadOnlyList<StationpediaCombustionOutput> Outputs, double? AutoIgnitionC, double? OutputTemperatureC);

/// <summary>
/// A Stationpedia page: the in-game encyclopedia of items, structures, gases,
/// and mechanics. <see cref="Text"/> is the full page body. <see cref="UsedIn"/>
/// lists what consumes the subject, <see cref="HowToBuild"/> how to make
/// it, <see cref="BuildSteps"/> how to place it stage by stage, and
/// <see cref="Combustion"/> how it burns (all null when the page states none).
/// Numeric specs hold raw game numbers when the subject states any, else null:
/// power generation in watts, power storage in joules, power draw in watts,
/// fuel energy per second in watts for solid fuels, and the tick length in
/// seconds that burn ticks last. <see cref="Logic"/> lists the logic
/// variables the subject reads or writes, and <see cref="Connections"/>
/// lists its data and cable ports (both null when the page states none).
/// </summary>
public sealed record StationpediaEntry(string Key, string Title, string Description, string Text, IReadOnlyList<StationpediaUse>? UsedIn, IReadOnlyList<StationpediaMake>? HowToBuild, IReadOnlyList<StationpediaCombustion>? Combustion = null, IReadOnlyList<string>? BuildSteps = null, double? PowerGenerationW = null, double? PowerStorageJ = null, double? PowerDrawW = null, double? FuelEnergyPerSecondW = null, double? TickSeconds = null, IReadOnlyList<StationpediaLogic>? Logic = null, IReadOnlyList<StationpediaConnection>? Connections = null);
