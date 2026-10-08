using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Repository;

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

    [SageGet("/players", "Describe the player's health, suit, and position.")]
    public SageResponse DescribePlayer()
    {
        var result = _commands.DescribePlayer().Execute(new NoRequest());
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Player status is unavailable.");
        }

        return SageResponse.Ok(result.Player);
    }

    [SageGet("/INDEX.md", "Read this API description as text.")]
    [SageGet("/")]
    public SageResponse Index()
    {
        return new SageResponse(200, SageIndex.Describe());
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
        var result = _commands.GetWorld().Execute(new NoRequest());
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "World info is unavailable.");
        }

        return SageResponse.Ok(result.World);
    }

    [SageGet("/inventory", "Search what the player carries and wears.")]
    public SageResponse Inventory(string? q)
    {
        return SageResponse.Ok(_commands.SearchInventory().Execute(new SearchInventoryRequest(q)).Items);
    }

    [SageGet("/devices", "Search devices by name, network, or room.")]
    public SageResponse Devices(string? q, string? network, string? logic, string? room)
    {
        return SageResponse.Ok(_commands.SearchDevices().Execute(new SearchDevicesRequest(
            q,
            Long(network),
            Bool(logic),
            Long(room))).Devices);
    }

    [SageGet("/devices/{device}", "Read one device's values and settings.")]
    public SageResponse DescribeDevice(string? device)
    {
        var result = _commands.DescribeDevice().Execute(new DescribeDeviceRequest(device));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Device not found.");
        }

        return SageResponse.Ok(result.Details);
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

        var result = _commands.SearchThings().Execute(new SearchThingsRequest(q, null, distance));
        return SageResponse.Ok(new ThingsResult(result.Things, result.Origin, result.Facing));
    }

    [SageGet("/things/{thing}/slots", "List the slots of one thing.")]
    public SageResponse Slots(string? thing)
    {
        var result = _commands.ReadThingSlots().Execute(new ReadThingSlotsRequest(thing));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Thing not found.");
        }

        return SageResponse.Ok(result.Slots);
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

        var room = _commands.ReadRoom().Execute(new ReadRoomRequest(roomId));
        if (room.Status != Status.SUCCESS)
        {
            return Failure(room, "Room not found.");
        }

        return SageResponse.Ok(room.Room);
    }

    [SageGet("/items", "Search items by name.")]
    public SageResponse Items(string? q)
    {
        return SageResponse.Ok(_commands.SearchItems().Execute(new SearchItemsRequest(q)).Items);
    }

    [SageGet("/structures", "Search structures by name.")]
    public SageResponse Structures(string? q)
    {
        return SageResponse.Ok(_commands.SearchStructures().Execute(new SearchStructuresRequest(q)).Structures);
    }

    [SageGet("/recipes", "Search recipes by name.")]
    public SageResponse Recipes(string? q)
    {
        return SageResponse.Ok(_commands.SearchRecipes().Execute(new SearchRecipesRequest(q)).Recipes);
    }

    [SageGet("/stationpedia", "Search help pages by name.")]
    public SageResponse Stationpedia(string? q)
    {
        return SageResponse.Ok(_commands.SearchStationpedia().Execute(new SearchStationpediaRequest(q)).Pages);
    }

    [SageGet("/stationpedia/{key}", "Read one help page.")]
    public SageResponse ReadStationpedia(string? key)
    {
        var result = _commands.ReadStationpedia().Execute(new ReadStationpediaRequest(key));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Page not found.");
        }

        return SageResponse.Ok(result.Page);
    }

    [SageGet("/instructions", "List the IC10 instructions.")]
    public SageResponse ListInstructions(string? instruction)
    {
        var result = _commands.ListInstructions().Execute(new ListInstructionsRequest(instruction));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "No instructions.");
        }

        return SageResponse.Ok(result.Instructions);
    }

    [SageGet("/circuit-housings/{device}", "Read the code from an IC10 circuit housing.")]
    public SageResponse ReadHousing(string? device)
    {
        var deviceId = Long(device);
        if (!deviceId.HasValue)
        {
            return SageResponse.Error(400, "device must be a thing id.");
        }

        var result = _commands.ReadHousing().Execute(new ReadHousingRequest(deviceId.Value));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Housing read failed.");
        }

        return SageResponse.Ok(result.Program);
    }

    [SageGet("/computer/{computer}", "Read a computer's editor text and housing.")]
    public SageResponse ReadComputer(string? computer)
    {
        var computerId = Long(computer);
        if (!computerId.HasValue)
        {
            return SageResponse.Error(400, "computer must be a thing id.");
        }

        var result = _commands.ReadComputer().Execute(computerId.Value);
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Computer read failed.");
        }

        return SageResponse.Ok(new ComputerReadBody(
            result.Name ?? string.Empty, result.Device, result.SourceCode ?? string.Empty, result.Hash ?? string.Empty));
    }

    /// <summary>The wire answer of <c>GET /sage/computer/{computer}</c>, shaped for callers.</summary>
    public sealed record ComputerReadBody(string Name, long Device, string SourceCode, string Hash);

    /// <summary>
    /// Maps a failed command answer to HTTP: 404 for a missed id, 400 for a
    /// bad ask, 500 for anything else, each with the command's sentences.
    /// </summary>
    private static SageResponse Failure(BaseResult result, string fault)
    {
        switch (result.Status)
        {
            case Status.NOT_FOUND:
                return SageResponse.Error(404, Sentence(result, "Not found."));
            case Status.BAD_REQUEST:
                return SageResponse.Error(400, Sentence(result, "Bad request."));
            default:
                return SageResponse.Error(500, Sentence(result, fault));
        }
    }

    private static string Sentence(BaseResult result, string fallback)
    {
        return result.Errors.Count == 0 ? fallback : string.Join(" ", result.Errors);
    }

    [SagePut("/computer/{computer}", "Write a computer's editor screen, guarded by the pointed chip's hash.")]
    public SageResponse WriteComputer(string? body, string? computer)
    {
        var computerId = Long(computer);
        if (!computerId.HasValue)
        {
            return SageResponse.Error(400, "computer must be a thing id.");
        }

        var request = ParseBody(body);
        if (request.SourceCode is null)
        {
            return SageResponse.Error(400, "sourceCode is required.");
        }

        return Answer(
            _commands.WriteComputer().Execute(new WriteComputerRequest(computerId.Value, request.SourceCode, request.Hash)),
            "Computer write failed.");
    }

    [SagePut("/computer/{computer}/import", "Pull a housing chip into a computer's editor.")]
    public SageResponse ImportComputer(string? body, string? computer)
    {
        var computerId = Long(computer);
        if (!computerId.HasValue)
        {
            return SageResponse.Error(400, "computer must be a thing id.");
        }

        var request = ParseBody(body);
        var deviceId = Long(request.Device);
        if (!deviceId.HasValue)
        {
            return string.IsNullOrWhiteSpace(request.Device)
                ? SageResponse.Error(400, "device is required to import.")
                : SageResponse.Error(400, "device must be a thing id.");
        }

        return Answer(
            _commands.ImportChip().Execute(new ImportChipRequest(computerId.Value, deviceId.Value)),
            "Import failed.");
    }

    [SagePut("/computer/{computer}/export", "Burn a computer's editor text onto a housing chip, hash-verified.")]
    public SageResponse ExportComputer(string? body, string? computer)
    {
        var computerId = Long(computer);
        if (!computerId.HasValue)
        {
            return SageResponse.Error(400, "computer must be a thing id.");
        }

        var request = ParseBody(body);
        var deviceId = Long(request.Device);
        if (!deviceId.HasValue)
        {
            return string.IsNullOrWhiteSpace(request.Device)
                ? SageResponse.Error(400, "device is required to export.")
                : SageResponse.Error(400, "device must be a thing id.");
        }

        return Answer(
            _commands.ExportChip().Execute(new ExportChipRequest(computerId.Value, deviceId.Value)),
            "Export failed.");
    }

    [SagePut("/computer/{computer}/select", "Point a computer's dropdown at one housing on its network.")]
    public SageResponse SelectComputer(string? body, string? computer)
    {
        var computerId = Long(computer);
        if (!computerId.HasValue)
        {
            return SageResponse.Error(400, "computer must be a thing id.");
        }

        var request = ParseBody(body);
        var deviceId = Long(request.Device);
        if (!deviceId.HasValue)
        {
            return string.IsNullOrWhiteSpace(request.Device)
                ? SageResponse.Error(400, "device is required to select.")
                : SageResponse.Error(400, "device must be a thing id.");
        }

        return Answer(
            _commands.SelectHousing().Execute(new SelectHousingRequest(computerId.Value, deviceId.Value)),
            "Select failed.");
    }

    /// <summary>
    /// One button's answer to HTTP: success ships what ended up where, the
    /// rest ship the command's sentences through <see cref="Failure"/>.
    /// </summary>
    private SageResponse Answer(ComputerActionResult result, string fault)
    {
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, fault);
        }

        return SageResponse.Ok(new ComputerWriteBody(result.Device ?? "computer", result.Lines));
    }

    /// <summary>The wire answer of <c>PUT /sage/computer/{computer}</c>: what holds how many lines.</summary>
    public sealed record ComputerWriteBody(string Device, int Lines);

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

        [JsonProperty("sourceCode")]
        public string? SourceCode { get; set; }

        [JsonProperty("hash")]
        public string? Hash { get; set; }

        [JsonProperty("export")]
        public bool? Export { get; set; }

        [JsonProperty("import")]
        public bool? Import { get; set; }
    }

    private sealed class ThingsResult
    {
        public ThingsResult(IReadOnlyList<ThingInfo> things, Point? origin, double? facing)
        {
            Things = things;
            Origin = origin;
            Facing = facing;
        }

        [JsonProperty("things")]
        public IReadOnlyList<ThingInfo> Things { get; }

        [JsonProperty("origin")]
        public Point? Origin { get; }

        [JsonProperty("facing")]
        public double? Facing { get; }
    }
}
