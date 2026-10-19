using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>
/// One “used in” line of a Stationpedia page: what consumes the subject
/// (<see cref="Title"/>, e.g. <c>Generator (Solid Fuel)</c>), with detail
/// when the game states any (e.g. <c>100 kW Energy</c> for fuels), and the
/// key of the consumer's own page when one exists.
/// </summary>
public sealed record StationpediaUse(string Title, string? Detail, string? Key);

/// <summary>
/// One “how to manufacture” line of a Stationpedia page: the station that
/// makes the subject (<see cref="Station"/>, e.g. <c>Advanced Furnace</c>),
/// its tier when the game states one, the requirements text (reagents with
/// quantities plus smelt conditions such as temperature and pressure), and
/// the key of the station's own page when one exists.
/// </summary>
public sealed record StationpediaMake(string Station, string? Tier, string? Detail, string? Key);

/// <summary>
/// A Stationpedia page: the in-game encyclopedia of items, structures, gases,
/// and mechanics. <see cref="Text"/> is the full page body. <see cref="UsedIn"/>
/// lists what consumes the subject and <see cref="HowToBuild"/> how to make
/// it (both null when the page states none).
/// </summary>
public sealed record StationpediaEntry(string Key, string Title, string Description, string Text, IReadOnlyList<StationpediaUse>? UsedIn, IReadOnlyList<StationpediaMake>? HowToBuild);
