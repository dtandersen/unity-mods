namespace StationeersAi.Entity;

/// <summary>A loose world thing: ore, ingot, plant, dropped item, wreckage. Positioned in meters.</summary>
public sealed record ThingInfo(string Name, string Type, long Thing, double X, double Y, double Z, long RoomId);
