using System;
using System.Collections.Generic;
using System.Linq;

namespace StationeersAi.Tool
{
    /// <summary>Dispatches tool calls to the registered tools.</summary>
    public sealed class ToolHandler
    {
        private readonly Dictionary<string, ITool> _tools;

        public ToolHandler(IEnumerable<ITool> tools)
        {
            _tools = tools.ToDictionary(tool => tool.Definition.Name, StringComparer.Ordinal);
            Definitions = _tools.Values.Select(tool => tool.Definition).ToArray();
        }

        /// <summary>Tool schema handed to the model.</summary>
        public IReadOnlyList<ToolDefinition> Definitions { get; }

        public ToolResult Handle(ToolCall call)
        {
            if (call == null)
            {
                return new ToolFailure("No tool call supplied.");
            }

            if (!_tools.TryGetValue(call.Name, out var tool))
            {
                return new ToolFailure($"Unknown tool '{call.Name}'.");
            }

            return tool.Execute(call.Arguments ?? ToolArguments.Empty);
        }
    }
}
