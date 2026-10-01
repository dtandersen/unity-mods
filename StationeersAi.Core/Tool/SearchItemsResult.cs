using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>Items matching a search.</summary>
public sealed record SearchItemsResult(IReadOnlyList<Item> Items);
