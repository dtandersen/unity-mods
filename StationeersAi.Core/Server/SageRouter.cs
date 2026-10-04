using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Server;

/// <summary>
/// Unity-free HTTP router for Sage. Maps <c>GET /sage/...</c> to the same
/// commands the chat assistant calls, and writes the result as JSON. The mod
/// hosts this over <c>HttpListener</c>; tests call <see cref="Handle"/>
/// straight with fakes.
/// </summary>
public sealed class SageRouter
{
    private readonly SageData _data;

    public SageRouter(SageData data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public SageResponse Handle(string method, string path, IDictionary<string, string> query, string body = null)
    {
        if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(path))
        {
            return SageResponse.Error(400, "Expected a method and path.");
        }

        var route = path.TrimEnd('/');
        if (route.StartsWith("/sage", StringComparison.OrdinalIgnoreCase))
        {
            route = route.Substring("/sage".Length);
            if (route.Length == 0)
            {
                route = "/";
            }
        }

        query = query ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            return HandleGet(route, query);
        }

        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            return HandlePost(route, query, body);
        }

        return SageResponse.Error(405, "Only GET and POST serve here.");
    }

    private SageResponse HandleGet(string route, IDictionary<string, string> query)
    {
        switch (route.ToLowerInvariant())
        {
            case "/status":
                return SageResponse.Ok(new GetStatusCommand(_data.Player).Execute());
            case "/world":
                return SageResponse.Ok(new GetWorldCommand(_data.Worlds).Execute());
            case "/inventory":
                return SageResponse.Ok(new SearchInventoryCommand(new InventoryIndex(_data.Player)).Execute(Q(query, "q")));
            case "/devices":
                return SageResponse.Ok(new SearchDevicesCommand(_data.Devices).Execute(
                    Q(query, "q"),
                    Long(Q(query, "network")),
                    Bool(Q(query, "logic")),
                    Long(Q(query, "room"))));
            case "/devices/describe":
                return Describe(Q(query, "device"));
            case "/things":
                return Things(Q(query, "q"), Q(query, "maxDistance"));
            case "/things/contents":
                return Contents(Q(query, "thing"));
            case "/room":
                return Room(Q(query, "id"));
            case "/items":
                return SageResponse.Ok(new SearchItemsCommand(new ItemIndex(_data.Items)).Execute(Q(query, "q")));
            case "/structures":
                return SageResponse.Ok(new SearchStructuresCommand(new StructureIndex(_data.Structures)).Execute(Q(query, "q")));
            case "/recipes":
                return SageResponse.Ok(new SearchRecipesCommand(new RecipeIndex(_data.Recipes)).Execute(Q(query, "q")));
            case "/stationpedia":
                return SageResponse.Ok(new SearchStationpediaCommand(new StationpediaIndex(_data.Stationpedia)).Execute(Q(query, "q")));
            case "/stationpedia/read":
                return ReadPage(Q(query, "key"));
            case "/mips":
                return SageResponse.Ok(new ReadMipsCommand(_data.Mips).Execute(Q(query, "instruction")));
            case "/ic":
                return SageResponse.Ok(new ReadIcCommand(_data.Ic).Execute(Q(query, "computer")));
            default:
                return SageResponse.Error(404, "No route serves " + route + ".");
        }
    }

    private SageResponse HandlePost(string route, IDictionary<string, string> query, string body)
    {
        switch (route.ToLowerInvariant())
        {
            case "/ic":
                return WriteIc(body, query);
            case "/ic/export":
                return ExportIc(body, query);
            default:
                return SageResponse.Error(404, "No route serves " + route + ".");
        }
    }

    private SageResponse Describe(string device)
    {
        var command = new DescribeDeviceCommand(_data.Inspector, _data.Devices);
        var details = command.Execute(device);
        if (details == null)
        {
            return SageResponse.Error(404, "Device '" + device + "' not found.");
        }

        return SageResponse.Ok(details);
    }

    private SageResponse Contents(string thing)
    {
        var contents = new ReadThingContentsCommand(_data.ThingContents).Execute(thing);
        if (contents == null)
        {
            return SageResponse.Error(404, "Thing '" + thing + "' not found.");
        }

        return SageResponse.Ok(contents);
    }

    private SageResponse Things(string query, string maxDistance)
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

        var command = new SearchThingsCommand(_data.Things, _data.Player);
        var result = command.Search(query, null, distance);
        return SageResponse.Ok(new ThingsResult(result.Things, result.Origin, result.Facing));
    }

    private SageResponse Room(string id)
    {
        long? roomId = Long(id);
        if (!string.IsNullOrWhiteSpace(id) && !roomId.HasValue)
        {
            return SageResponse.Error(400, "id must be a number.");
        }

        var room = new ReadRoomCommand(_data.Rooms, _data.Player).Execute(roomId);
        if (room == null)
        {
            return SageResponse.Error(404, "Room not found.");
        }

        return SageResponse.Ok(room);
    }

    private SageResponse ReadPage(string key)
    {
        var page = new ReadStationpediaCommand(_data.Stationpedia).Execute(key);
        if (page == null)
        {
            return SageResponse.Error(404, "Page '" + key + "' not found.");
        }

        return SageResponse.Ok(page);
    }

    private SageResponse WriteIc(string body, IDictionary<string, string> query)
    {
        var request = ParseBody(body);
        var computer = request.Computer ?? Q(query, "computer");
        var result = new WriteIcCommand(_data.Ic).Execute(computer, request.Code ?? string.Empty);
        if (result == null || !result.Written)
        {
            return SageResponse.Error(404, result != null ? result.Error ?? "Write fell through." : "Write fell through.");
        }

        return SageResponse.Ok(result);
    }

    private SageResponse ExportIc(string body, IDictionary<string, string> query)
    {
        var request = ParseBody(body);
        var computer = request.Computer ?? Q(query, "computer");
        var device = request.Device ?? Q(query, "device");
        var result = new ExportIcCommand(_data.Export).Execute(computer, device);
        if (result == null || !result.Written)
        {
            return SageResponse.Error(404, result != null ? result.Error ?? "Export fell through." : "Export fell through.");
        }

        return SageResponse.Ok(result);
    }

    private static IcRequest ParseBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
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

    private static string Q(IDictionary<string, string> query, string name)
    {
        string value;
        if (query.TryGetValue(name, out value))
        {
            return value;
        }

        return null;
    }

    private static long? Long(string value)
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

    private static bool? Bool(string value)
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
        public string Computer { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    private sealed class ThingsResult
    {
        public ThingsResult(IReadOnlyList<ThingInfo> things, OriginPoint origin, double? facing)
        {
            Things = things;
            Origin = origin;
            Facing = facing;
        }

        [JsonProperty("things")]
        public IReadOnlyList<ThingInfo> Things { get; }

        [JsonProperty("origin")]
        public OriginPoint Origin { get; }

        [JsonProperty("facing")]
        public double? Facing { get; }
    }
}
