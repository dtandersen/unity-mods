using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: read a pipe network's air and members.</summary>
public sealed class ReadPipeNetworkTool : ITool
{
    private readonly ReadPipeNetworkCommand _command;

    public ReadPipeNetworkTool(ReadPipeNetworkCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "read_pipenetwork",
        "Read a pipe network: pressure, temperature, volume, gas moles, and what sits on it. Pass a pipe network id, or a device label or id joined to it (e.g. a connector, pipe, or tank).",
        new[]
        {
            new ToolParameter("pipenetwork", "The pipe network id, e.g. '21665'.", Required: false),
            new ToolParameter("device", "A device label or id joined to the pipe network, e.g. '20211'.", Required: false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var networkText = arguments.Get("pipenetwork");
        var device = arguments.Get("device");

        long? pipeNetworkId = null;
        if (!string.IsNullOrWhiteSpace(networkText))
        {
            long id;
            if (!long.TryParse(networkText.Trim(), out id))
            {
                return new ToolFailure($"Pipe network '{networkText}' is not a number.");
            }

            pipeNetworkId = id;
        }

        if (pipeNetworkId == null && string.IsNullOrWhiteSpace(device))
        {
            return new ToolFailure("Missing required argument 'pipenetwork' or 'device'.");
        }

        var answer = _command.Execute(new ReadPipeNetworkRequest(pipeNetworkId, device));
        if (answer.Status != Status.SUCCESS)
        {
            return new ToolFailure(ToolAnswer.Line(answer, "Pipe network not found."));
        }

        return new ToolSuccess<ReadPipeNetworkResult>(
            new ReadPipeNetworkResult(answer.PipeNetworks),
            string.Join("\n\n", answer.PipeNetworks.Select(Describe)));
    }

    private static readonly string[] LiquidGases = new[]
    {
        "Water",
        "PollutedWater",
        "LiquidNitrogen",
        "LiquidOxygen",
        "LiquidHydrogen",
        "LiquidMethane",
        "LiquidCarbonDioxide",
        "LiquidPollutant",
        "LiquidNitrousOxide",
        "LiquidHydrazine",
        "LiquidAlcohol",
        "LiquidSodiumChloride",
        "LiquidSilanol",
        "LiquidHydrochloricAcid",
        "LiquidOzone",
    };

    private static string Describe(PipeNetworkDetails info)
    {
        var lines = new List<string>
        {
            $"Pipe network {info.PipeNetworkId}" + (string.IsNullOrWhiteSpace(info.ContentType) ? string.Empty : $" ({info.ContentType})"),
            $"Temperature: {info.TemperatureC:0.#}°C",
            $"Pressure: {info.PressureKpa:0.#} kPa",
            $"Volume: {info.VolumeLitres:0.#} L",
            $"Total: {info.TotalMoles:0.#} mol",
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

        var members = info.Members ?? new string[0];
        if (members.Count == 0)
        {
            lines.Add("Members: none listed.");
        }
        else
        {
            lines.Add($"Members ({members.Count}):");
            lines.AddRange(members.Take(20));
        }

        return string.Join("\n", lines);
    }
}
