using Sage.Client.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sage.Command;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Server;

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

    [SageGet("/structures", "Search built structures by name, power or data network, or room.")]
    public SageResponse Structures(StructureSearchRequest request)
    {
        return SageResponse.Ok(_commands.SearchStructures().Execute(new SearchStructuresRequest(
            request.Query,
            request.PowerNetworkId,
            request.DataNetworkId,
            request.Logic,
            request.Room)).Structures);
    }

    [SageGet("/structures/{structure}", "Read one structure's values and settings.")]
    public SageResponse DescribeStructure(string? structure)
    {
        var result = _commands.DescribeStructure().Execute(new DescribeStructureRequest(structure));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Structure not found.");
        }

        return SageResponse.Ok(result.Details);
    }

    [SageGet("/things", "Search nearby things by name.")]
    public SageResponse Things(ThingSearchRequest request)
    {
        var result = _commands.SearchThings().Execute(new SearchThingsRequest(request.Query, null, request.MaxDistance));
        return SageResponse.Ok(new ThingsResult(result.Things, result.Origin, result.Facing));
    }

    [SageGet("/things/{thing}", "Read one live thing: a built structure returns its structure values, a loose item returns quantity and position.")]
    public SageResponse DescribeThing(string? thing)
    {
        var result = _commands.DescribeThing().Execute(new DescribeThingRequest(thing));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Thing not found.");
        }

        return SageResponse.Ok(result.Details);
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
    public SageResponse Rooms(RoomReadRequest request)
    {
        var room = _commands.ReadRoom().Execute(new ReadRoomRequest(request.Id));
        if (room.Status != Status.SUCCESS)
        {
            return Failure(room, "Room not found.");
        }

        return SageResponse.Ok(room.Room);
    }

    [SageGet("/pipe-networks", "Read pipe networks joined to a device.")]
    [SageGet("/pipe-networks/{pipenetwork}", "Read one pipe network's air and members.")]
    public SageResponse PipeNetworks(PipeNetworkReadRequest request)
    {
        var result = _commands.ReadPipeNetwork().Execute(new ReadPipeNetworkRequest(request.PipeNetwork, request.Device));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Pipe network not found.");
        }

        if (request.PipeNetwork.HasValue)
        {
            return SageResponse.Ok(result.PipeNetworks.Count == 0 ? null : result.PipeNetworks[0]);
        }

        return SageResponse.Ok(result.PipeNetworks);
    }

    [SageGet("/items", "Search items by name.")]
    public SageResponse Items(string? q)
    {
        return SageResponse.Ok(_commands.SearchItems().Execute(new SearchItemsRequest(q)).Items);
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
    public SageResponse ReadHousing(CircuitHousingReadRequest request)
    {
        var result = _commands.ReadHousing().Execute(new ReadHousingRequest(request.Device));
        if (result.Status != Status.SUCCESS)
        {
            return Failure(result, "Housing read failed.");
        }

        return SageResponse.Ok(result.Program);
    }

    [SageGet("/computer/{computer}", "Read a computer's editor text and housing.")]
    public SageResponse ReadComputer(ComputerReadRequest request)
    {
        var result = _commands.ReadComputer().Execute(request.Computer);
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

    [SagePut("/computer/{computer}", "Write a computer's editor screen, guarded by the selected chip's hash.")]
    public SageResponse WriteComputer(ComputerWriteRequest request)
    {
        if (request.SourceCode is null)
        {
            return SageResponse.Error(400, "sourceCode is required.");
        }

        return Answer(
            _commands.WriteComputer().Execute(new WriteComputerRequest(request.Computer, request.SourceCode, request.Hash)),
            "Computer write failed.");
    }

    [SagePut("/computer/{computer}/import", "Pull the selected housing chip into a computer's editor.")]
    public SageResponse ImportComputer(ComputerImportRequest request)
    {
        return Answer(
            _commands.ImportChip().Execute(new ImportChipRequest(request.Computer)),
            "Import failed.");
    }

    [SagePut("/computer/{computer}/export", "Burn a computer's editor text onto a housing chip, hash-verified.")]
    public SageResponse ExportComputer(ComputerExportRequest request)
    {
        if (!request.Device.HasValue)
        {
            return SageResponse.Error(400, "device is required to export.");
        }

        return Answer(
            _commands.ExportChip().Execute(new ExportChipRequest(request.Computer, request.Device.Value)),
            "Export failed.");
    }

    [SagePut("/computer/{computer}/select", "Point a computer's dropdown at one housing on its network.")]
    public SageResponse SelectComputer(ComputerSelectRequest request)
    {
        if (!request.Device.HasValue)
        {
            return SageResponse.Error(400, "device is required to select.");
        }

        return Answer(
            _commands.SelectHousing().Execute(new SelectHousingRequest(request.Computer, request.Device.Value)),
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
