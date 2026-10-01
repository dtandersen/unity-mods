namespace StationeersAi.Entity;

/// <summary>
/// A Stationpedia page: the in-game encyclopedia of items, structures, gases,
/// and mechanics. <see cref="Text"/> is the full page body.
/// </summary>
public sealed record StationpediaEntry(string Key, string Title, string Description, string Text);
