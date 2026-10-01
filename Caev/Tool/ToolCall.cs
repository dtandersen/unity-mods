namespace Caev.Tools;

/// <summary>A tool invocation requested by the model.</summary>
public sealed record ToolCall(string Id, string Name, ToolArguments Arguments);
