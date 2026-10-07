namespace StationeersAi.Repository;

/// <summary>The housing a lookup resolved to, or why it resolved to none.</summary>
public sealed record HousingLookup(IHousing? Housing, string? Error);
