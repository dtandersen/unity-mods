using StationeersAi.Command;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: read one Stationpedia page by key. Returns the full page text.</summary>
public sealed class ReadStationpediaTool : ITool
{
    private readonly ReadStationpediaCommand _command;

    public ReadStationpediaTool(ReadStationpediaCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "read_stationpedia",
        "Read the full text of one Stationpedia page. Find pages first with search_stationpedia.",
        new[] { new ToolParameter("key", "The page Key from search_stationpedia, e.g. 'ItemSteel'.") });

    public ToolResult Execute(ToolArguments arguments)
    {
        var key = arguments.Get("key");
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ToolFailure("Missing required argument 'key'.");
        }

        var entry = _command.Execute(key);
        if (entry == null)
        {
            return new ToolFailure($"No Stationpedia page found for key '{key}'.");
        }

        var text = string.IsNullOrWhiteSpace(entry.Text)
            ? (entry.Description ?? string.Empty)
            : entry.Text;

        return new ToolSuccess<ReadStationpediaResult>(
            new ReadStationpediaResult(entry),
            $"{entry.Title}\n{text}".Trim());
    }
}
