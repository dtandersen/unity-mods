using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>The things found, with positions.</summary>
public sealed record SearchThingsResult(IReadOnlyList<ThingInfo> Things);
