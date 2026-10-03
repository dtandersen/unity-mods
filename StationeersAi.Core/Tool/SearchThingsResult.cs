using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>The things found, with positions.</summary>
public sealed record SearchThingsResult(IReadOnlyList<ThingInfo> Things);
