using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Command;
using Sage.Entity;
using Caev.Tools;

namespace Sage.Tool;

/// <summary>Tool: read a room's atmospherics (temperature, pressure, gas moles).</summary>
public sealed class ReadRoomTool : ITool
{
    private readonly ReadRoomCommand _command;

    public ReadRoomTool(ReadRoomCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "read_room",
        "Read a room's atmospherics: type, temperature, pressure, volume, and gas moles. Pass a room id from the Room column of search_structures. Omit room to read the room you stand in.",
        new[]
        {
            new ToolParameter("room", "The room id, e.g. '5'. Omit it to read your own room.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var query = arguments.Get("room");
        long? roomId = null;
        if (query is not null && !string.IsNullOrWhiteSpace(query))
        {
            long id;
            if (!long.TryParse(query.Trim(), out id))
            {
                return new ToolFailure($"Room '{query}' is not a number.");
            }

            roomId = id;
        }

        var answer = _command.Execute(new ReadRoomRequest(roomId));
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Room not found."));
        }

        var info = answer.Room!;

        return new ToolSuccess<ReadRoomResult>(
            new ReadRoomResult(info),
            Describe(info));
    }

    private static readonly string[] LiquidGases = new[]
    {
        "Water",
        "LiquidNitrogen",
        "LiquidOxygen",
        "LiquidHydrogen",
        "LiquidMethane",
        "LiquidCarbonDioxide",
        "LiquidPollutant",
        "LiquidNitrousOxide",
    };

    private static string Describe(RoomAtmosphere info)
    {
        var lines = new List<string>
        {
            $"Room {info.RoomId}",
            $"Temperature: {info.TemperatureC:0.#}\u00B0C",
            $"Pressure: {info.PressureKpa:0.#} kPa",
            $"Volume: {info.VolumeLitres:0.#} L",
        };

        var gases = info.Gases ?? new GasMoles[0];
        var vapors = gases.Where(gas => !LiquidGases.Contains(gas.MoleType)).ToArray();
        var liquids = gases.Where(gas => LiquidGases.Contains(gas.MoleType)).ToArray();
        if (vapors.Length > 0)
        {
            lines.Add("Gas:");
            lines.AddRange(vapors.Select(gas => $"{gas.MoleType}: {gas.Moles:0.#} mol"));
        }

        if (liquids.Length > 0)
        {
            lines.Add("Liquid:");
            lines.AddRange(liquids.Select(gas => $"{gas.MoleType}: {gas.Moles:0.#} mol"));
        }

        if (vapors.Length == 0 && liquids.Length == 0)
        {
            lines.Add("Vacuum");
        }

        return string.Join("\n", lines);
    }
}
