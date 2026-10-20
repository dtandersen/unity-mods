using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Command;
using Sage.Entity;
using Caev.Tools;

namespace Sage.Tool;

/// <summary>Tool: report the world (setup, orbit, sunlight, outside air).</summary>
public sealed class GetWorldTool : ITool
{
    private readonly GetWorldCommand _command;

    public GetWorldTool(GetWorldCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "get_world",
        "Report the world: name, gravity, day length, orbit, sunlight, and outside air. Takes no arguments.",
        new ToolParameter[0]);

    public ToolResult Execute(ToolArguments arguments)
    {
        var answer = _command.Execute(new NoRequest());
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "World info is unavailable."));
        }

        var info = answer.World!;

        return new ToolSuccess<GetWorldResult>(
            new GetWorldResult(info),
            Describe(info));
    }

    private static string Describe(WorldInfo info)
    {
        var lines = new List<string>
        {
            $"World: {info.WorldName ?? "unknown"}",
            $"Gravity: {Math.Abs(info.Gravity):0.0} m/s\u00B2, Day {info.DaysPast}, Lava: {(info.HasLava ? "yes" : "no")}",
        };

        if (info.DayLength != null || info.YearLength != null)
        {
            lines.Add($"Day length: {info.DayLength ?? "unknown"}, Year: {info.YearLength ?? "unknown"} ({info.DaysPerYear:0.0} days per year)");
        }

        lines.Add($"Orbit: e={info.Eccentricity:0.00}, a={info.SemiMajorAxisAu:0.00} AU, inclination {info.Inclination:0.0}\u00B0{(info.TidallyLocked ? ", tidally locked" : string.Empty)}");
        lines.Add($"Sun: {info.SolarIrradiance:0} W/m\u00B2 ({info.EarthSolarRatio:0.00}x Earth), time {info.TimeOfDay:0%}{(info.IsEclipse ? ", eclipse" : string.Empty)}");

        if (info.OutsideTemperatureC != null || info.OutsidePressureKpa != null)
        {
            lines.Add($"Outside air: {(info.OutsideTemperatureC != null ? $"{info.OutsideTemperatureC:0.#}\u00B0C" : "unknown")}, {(info.OutsidePressureKpa != null ? $"{info.OutsidePressureKpa:0.#} kPa" : "unknown")}");
        }

        if (info.OutsideAir != null && info.OutsideAir.Count > 0)
        {
            lines.Add("Air mix: " + string.Join(", ", info.OutsideAir.Select(entry => $"{entry.MoleType} {entry.Percent:0.#}%")));
        }

        return string.Join("\n", lines);
    }
}
