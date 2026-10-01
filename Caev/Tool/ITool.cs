namespace Caev.Tools;

/// <summary>
/// A tool the model can call. Owns its schema, argument handling and the
/// shaping of its typed result / model-facing text, and delegates the actual
/// work to a command.
/// </summary>
public interface ITool
{
    ToolDefinition Definition { get; }

    ToolResult Execute(ToolArguments arguments);
}
