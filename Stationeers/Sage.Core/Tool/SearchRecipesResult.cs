using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>Recipes matching a search.</summary>
public sealed record SearchRecipesResult(IReadOnlyList<Recipe> Recipes);
