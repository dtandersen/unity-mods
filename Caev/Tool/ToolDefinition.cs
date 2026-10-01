using System.Collections.Generic;

namespace Caev.Tools;

/// <summary>The model-facing description of a tool.</summary>
public sealed record ToolDefinition(
    string Name,
    string Description,
    IReadOnlyList<ToolParameter> Parameters);
