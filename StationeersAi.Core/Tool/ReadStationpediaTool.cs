using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
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

        var answer = _command.Execute(new ReadStationpediaRequest(key));
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Page not found."));
        }

        var entry = answer.Page!;

        var text = string.IsNullOrWhiteSpace(entry.Text)
            ? (entry.Description ?? string.Empty)
            : entry.Text;

        var lines = new List<string> { $"{entry.Title}\n{text}".Trim() };
        var uses = (entry.UsedIn ?? Array.Empty<StationpediaUse>()).ToArray();
        if (uses.Length > 0)
        {
            lines.Add("Used in:");
            foreach (var use in uses)
            {
                lines.Add(string.IsNullOrWhiteSpace(use.Detail) ? use.Title : $"{use.Title} - {use.Detail}");
            }
        }

        var makes = (entry.HowToBuild ?? Array.Empty<StationpediaMake>()).ToArray();
        if (makes.Length > 0)
        {
            lines.Add("Made in:");
            foreach (var make in makes)
            {
                var station = string.IsNullOrWhiteSpace(make.Tier) ? make.Station : $"{make.Station} ({make.Tier})";
                lines.Add(string.IsNullOrWhiteSpace(make.Detail) ? station : $"{station} - {make.Detail}");
            }
        }

        var steps = (entry.BuildSteps ?? Array.Empty<string>()).ToArray();
        if (steps.Length > 0)
        {
            lines.Add("Build:");
            foreach (var step in steps)
            {
                lines.Add(step);
            }
        }

        var burns = (entry.Combustion ?? Array.Empty<StationpediaCombustion>()).ToArray();
        if (burns.Length > 0)
        {
            lines.Add("Combustion:");
            foreach (var burn in burns)
            {
                lines.Add(FormatCombustion(burn));
            }
        }

        return new ToolSuccess<ReadStationpediaResult>(
            new ReadStationpediaResult(entry),
            string.Join("\n", lines));
    }

    internal static string FormatCombustion(StationpediaCombustion burn)
    {
        var outputs = (burn.Outputs ?? System.Array.Empty<StationpediaCombustionOutput>())
            .Select(o => $"{FormatAmount(o.Quantity)} x {o.Gas}");
        var line = $"{FormatAmount(burn.FuelAmount)} x {burn.Fuel} + {FormatAmount(burn.OxidiserAmount)} x {burn.Oxidiser} -> {string.Join(" + ", outputs)}";
        if (burn.AutoIgnitionC.HasValue)
        {
            line += $"; Auto Ignition {FormatAmount(burn.AutoIgnitionC.Value)} °C";
        }

        if (burn.OutputTemperatureC.HasValue)
        {
            line += $"; {FormatAmount(burn.OutputTemperatureC.Value)} °C";
        }

        return line;
    }

    internal static string FormatAmount(double amount)
    {
        return amount == System.Math.Truncate(amount)
            ? ((long)amount).ToString()
            : amount.ToString("0.##");
    }
}
