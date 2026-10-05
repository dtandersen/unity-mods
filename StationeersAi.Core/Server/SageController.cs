using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StationeersAi.Entity;

namespace StationeersAi.Server;

/// <summary>
/// Runs one Sage command per call and maps the result to a <see cref="SageResponse"/>.
/// Holds the not-found and bad-request answers, so the router only matches
/// paths and pulls values from the query. Holds no game state.
/// </summary>
public sealed class SageController
{
    private readonly SageCommands _commands;

    public SageController(SageCommands commands)
    {
        _commands = commands;
    }

    [SageGet("/players", "Read the player's health, suit, and position.")]
    public SageResponse Players()
    {
        return SageResponse.Ok(_commands.GetPlayer().Execute());
    }

    [SageGet("/openapi", "Read this API description.")]
    public SageResponse OpenApi(string? pretty)
    {
        if (Truthy(pretty))
        {
            return SageResponse.OkPretty(SageOpenApi.Describe());
        }

        return SageResponse.Ok(SageOpenApi.Describe());
    }

    [SageGet("/world", "Read the loaded world name and mode.")]
    public SageResponse World()
    {
        return SageResponse.Ok(_commands.GetWorld().Execute());
    }

    [SageGet("/inventory", "Search what the player carries and wears.")]
    public SageResponse Inventory(string? q)
    {
        return SageResponse.Ok(_commands.SearchInventory().Execute(q));
    }

    [SageGet("/devices", "Search devices by name, network, or room.")]
    public SageResponse Devices(string? q, string? network, string? logic, string? room)
    {
        return SageResponse.Ok(_commands.SearchDevices().Execute(
            q,
            Long(network),
            Bool(logic),
            Long(room)));
    }

    [SageGet("/devices/{device}", "Read one device's values and settings.")]
    public SageResponse DescribeDevice(string? device)
    {
        var details = _commands.DescribeDevice().Execute(device);
        if (details == null)
        {
            return SageResponse.Error(404, "Device '" + device + "' not found.");
        }

        return SageResponse.Ok(details);
    }

    [SageGet("/things", "Search nearby things by name.")]
    public SageResponse Things(string? q, string? maxDistance)
    {
        double? distance = null;
        if (!string.IsNullOrWhiteSpace(maxDistance))
        {
            double parsed;
            if (!double.TryParse(maxDistance, out parsed))
            {
                return SageResponse.Error(400, "maxDistance must be a number.");
            }

            distance = parsed;
        }

        var result = _commands.SearchThings().Search(q, null, distance);
        return SageResponse.Ok(new ThingsResult(result.Things, result.Origin, result.Facing));
    }

    [SageGet("/things/{thing}/slots", "List the slots of one thing.")]
    public SageResponse Slots(string? thing)
    {
        var slots = _commands.ReadThingSlots().Execute(thing);
        if (slots == null)
        {
            return SageResponse.Error(404, "Thing '" + thing + "' not found.");
        }

        return SageResponse.Ok(slots);
    }

    [SageGet("/rooms", "Read the air of the room the player stands in.")]
    [SageGet("/rooms/{id}", "Read the air and size of one room.")]
    public SageResponse Rooms(string? id)
    {
        long? roomId = Long(id);
        if (!string.IsNullOrWhiteSpace(id) && !roomId.HasValue)
        {
            return SageResponse.Error(400, "id must be a number.");
        }

        var room = _commands.ReadRoom().Execute(roomId);
        if (room == null)
        {
            return SageResponse.Error(404, "Room not found.");
        }

        return SageResponse.Ok(room);
    }

    [SageGet("/items", "Search items by name.")]
    public SageResponse Items(string? q)
    {
        return SageResponse.Ok(_commands.SearchItems().Execute(q));
    }

    [SageGet("/structures", "Search structures by name.")]
    public SageResponse Structures(string? q)
    {
        return SageResponse.Ok(_commands.SearchStructures().Execute(q));
    }

    [SageGet("/recipes", "Search recipes by name.")]
    public SageResponse Recipes(string? q)
    {
        return SageResponse.Ok(_commands.SearchRecipes().Execute(q));
    }

    [SageGet("/stationpedia", "Search help pages by name.")]
    public SageResponse Stationpedia(string? q)
    {
        return SageResponse.Ok(_commands.SearchStationpedia().Execute(q));
    }

    [SageGet("/stationpedia/{key}", "Read one help page.")]
    public SageResponse ReadStationpedia(string? key)
    {
        var page = _commands.ReadStationpedia().Execute(key);
        if (page == null)
        {
            return SageResponse.Error(404, "Page '" + key + "' not found.");
        }

        return SageResponse.Ok(page);
    }

    [SageGet("/instructions", "Search the instruction list.")]
    public SageResponse Instructions(string? instruction)
    {
        return SageResponse.Ok(_commands.ReadInstructions().Execute(instruction));
    }

    [SageGet("/ic", "Read the code on the open computer.")]
    [SageGet("/ic/{computer}", "Read the code on one computer.")]
    public SageResponse ReadIc(string? computer)
    {
        return SageResponse.Ok(_commands.ReadIc().Execute(computer));
    }

    [SagePost("/ic/{computer}", "Write code to one computer.")]
    public SageResponse WriteIc(string? body, string? computer)
    {
        var request = ParseBody(body);
        computer = request.Computer ?? computer;
        var result = _commands.WriteIc().Execute(computer, request.Code ?? string.Empty);
        if (result == null || !result.Written)
        {
            return SageResponse.Error(404, result != null ? result.Error ?? "Write fell through." : "Write fell through.");
        }

        return SageResponse.Ok(result);
    }

    [SagePut("/ic/{computer}/export", "Send code from a computer to a device.")]
    public SageResponse ExportIc(string? body, string? computer, string? device)
    {
        var request = ParseBody(body);
        computer = request.Computer ?? computer;
        device = request.Device ?? device;
        var result = _commands.ExportIc().Execute(computer, device);
        if (result == null || !result.Written)
        {
            return SageResponse.Error(404, result != null ? result.Error ?? "Export fell through." : "Export fell through.");
        }

        return SageResponse.Ok(result);
    }

    private static bool Truthy(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Equals("0", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("false", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("no", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("off", StringComparison.OrdinalIgnoreCase);
    }

    private static IcRequest ParseBody(string? body)
    {
        if (body is null || string.IsNullOrWhiteSpace(body))
        {
            return new IcRequest();
        }

        try
        {
            return JsonConvert.DeserializeObject<IcRequest>(body) ?? new IcRequest();
        }
        catch (JsonException)
        {
            return new IcRequest();
        }
    }

    private static long? Long(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        long parsed;
        if (long.TryParse(value, out parsed))
        {
            return parsed;
        }

        return null;
    }

    private static bool? Bool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        bool parsed;
        if (bool.TryParse(value, out parsed))
        {
            return parsed;
        }

        if (value == "1")
        {
            return true;
        }

        if (value == "0")
        {
            return false;
        }

        return null;
    }

    private sealed class IcRequest
    {
        [JsonProperty("computer")]
        public string? Computer { get; set; }

        [JsonProperty("device")]
        public string? Device { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }

    private sealed class ThingsResult
    {
        public ThingsResult(IReadOnlyList<ThingInfo> things, OriginPoint? origin, double? facing)
        {
            Things = things;
            Origin = origin;
            Facing = facing;
        }

        [JsonProperty("things")]
        public IReadOnlyList<ThingInfo> Things { get; }

        [JsonProperty("origin")]
        public OriginPoint? Origin { get; }

        [JsonProperty("facing")]
        public double? Facing { get; }
    }
}
