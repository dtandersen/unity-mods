using System;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>Tool: read a full compendium page (lore or Munin's hint) by its topic.</summary>
    public sealed class ReadCompendiumTool : ITool
    {
        private readonly ReadCompendiumCommand _command;

        public ReadCompendiumTool(ReadCompendiumCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "read_compendium",
            "Read a compendium page in full by its exact topic, as returned by search_compendium.",
            new[] { new ToolParameter("topic", "The page topic to read, e.g. 'Boar hunting'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var topic = arguments.Get("topic");
            if (string.IsNullOrWhiteSpace(topic))
            {
                return new ToolFailure("Missing required argument 'topic'.");
            }

            var entry = _command.Execute(topic);
            if (entry == null)
            {
                return new ToolFailure($"No compendium page found for '{topic}'. Use search_compendium first.");
            }

            var content = $"{entry.Topic}\n\n{entry.Text}";
            return new ToolSuccess<ReadCompendiumResult>(
                new ReadCompendiumResult(entry.Topic, entry.Text),
                content);
        }
    }
}