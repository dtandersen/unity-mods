namespace Sage.Entity;

/// <summary>An ingredient required by a recipe, measured in reagent units.</summary>
public sealed record Ingredient(string ItemId, double Amount);
