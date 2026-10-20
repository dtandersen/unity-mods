using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>Items matching a search.</summary>
public sealed record SearchItemsResult(IReadOnlyList<Item> Items);
