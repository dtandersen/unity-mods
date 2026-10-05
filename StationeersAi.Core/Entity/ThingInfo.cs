namespace StationeersAi.Entity;

/// <summary>A loose world thing: ore, ingot, plant, dropped item, wreckage. Positioned in meters.</summary>
public sealed record ThingInfo(string Name, string? Type, long Thing, string? Quantity, double X, double Y, double Z, long RoomId);

/// <summary>One item inside a container thing (belt, crate): its slot, name, and quantity text.</summary>
public sealed record ThingSlot(string Slot, string Name, string? Quantity);
