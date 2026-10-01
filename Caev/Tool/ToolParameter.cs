namespace Caev.Tools;

/// <summary>Describes a single tool parameter for the model.</summary>
public sealed record ToolParameter(string Name, string Description, bool Required = true);
