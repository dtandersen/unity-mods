using System.Globalization;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>Tool: report the player's current position and biome.</summary>
    public sealed class GetLocationTool : ITool
    {
        private readonly GetLocationCommand _command;

        public GetLocationTool(GetLocationCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "get_location",
            "Get the player's current position and the biome they are in. Returns CSV with columns X, Y, Z, Biome. Use it for questions like 'where am I?'.",
            new ToolParameter[0]);

        public ToolResult Execute(ToolArguments arguments)
        {
            var location = _command.Execute();
            if (location == null)
            {
                return new ToolFailure("Player location is not available.");
            }

            var result = new LocationResult(location.X, location.Y, location.Z, location.Biome);
            var content = Csv.Build(
                new[] { "X", "Y", "Z", "Biome" },
                new[] { new[] { Number(result.X), Number(result.Y), Number(result.Z), result.Biome } });

            return new ToolSuccess<LocationResult>(result, content);
        }

        private static string Number(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}