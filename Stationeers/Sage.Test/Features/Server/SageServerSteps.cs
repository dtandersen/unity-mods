using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Shouldly;
using Sage.Entity;
using Sage.Server;
using Sage.Test.Support;

namespace Sage.Test.Features.Server;

[Binding]
public sealed class SageServerSteps
{
    private readonly ScenarioContext _context;
    private SageResponse _response;

    public SageServerSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    private static StructurePosition Position(string value)
    {
        var parts = value.Split(',');
        return new StructurePosition(
            double.Parse(parts[0], CultureInfo.InvariantCulture),
            double.Parse(parts[1], CultureInfo.InvariantCulture),
            double.Parse(parts[2], CultureInfo.InvariantCulture));
    }

    [Given("live structures exist:")]
    public void GivenLiveStructuresExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            World.Structures.Structures.Add(new StructureInfo(
                row["Name"],
                row["PrefabName"],
                long.Parse(row["Id"]),
                ParseConnections(row["Connections"]),
                bool.Parse(row["Logic"]),
                long.Parse(row["Room"]),
                Position(row["Position"])));
        }
    }

    private static List<NetworkConnection> ParseConnections(string value)
    {
        var connections = new List<NetworkConnection>();
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return connections;
        }

        foreach (var token in value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = token.Split(':');
            if (parts.Length != 2 || !long.TryParse(parts[1], out var id))
            {
                continue;
            }

            var label = parts[0];
            if (label.Length > 1 && (label[0] == 'p' || label[0] == 'd' || label[0] == 'c') && int.TryParse(label.Substring(1), out var port))
            {
                var kind = label[0] == 'p' ? "power" : label[0] == 'd' ? "data" : "chute";
                connections.Add(new NetworkConnection(kind, port, id));
            }
            else if (label == "input" || label == "input2" || label == "output" || label == "output2")
            {
                var index = label == "input" ? 0 : label == "input2" ? 1 : label == "output" ? 2 : 3;
                connections.Add(new NetworkConnection("pipe", index, id, label));
            }
        }

        return connections;
    }

    [Given("the MIPS reference contains {string} and {string}")]
    public void GivenTheMipsReferenceContains(string first, string second)
    {
        World.Mips.Instructions.Add(new Instruction(first, $"{first} r? device var", $"Does {first}."));
        World.Mips.Instructions.Add(new Instruction(second, second, $"Does {second}."));
    }

    [Given("the IC editor holds {string} on {string}")]
    public void GivenTheIcEditorHolds(string code, string computer)
    {
        World.Computer.Name = computer;
        World.Computer.EditorCode = code;
    }

    [Given("the housing holds {string} on {string}")]
    public void GivenTheHousingHolds(string code, string device)
    {
        World.Housing.Name = device;
        World.Housing.ChipCode = code;
    }

    [Given("the computer points at {string}")]
    public void GivenTheComputerPointsAt(string device)
    {
        World.Computer.SelectedDevice = device;
        // The same housing "Climate Control" carries thing id 2200 in the
        // circuit-housing routes these features exercise.
        World.Computer.SelectedThing = 2200;
    }

    [Given("the computer read faults")]
    public void GivenTheComputerReadFaults()
    {
        World.Computer.ReadFault = new InvalidOperationException("Editor read failed.");
    }

    [When(@"I GET (.+)")]
    public void WhenIGet(string request)
    {
        var router = new SageRouter(CommandsFrom(World), World.Log);
        Parse(request, out var path, out var query);
        _response = router.Handle("GET", path, query);
    }

    [When(@"I (POST|PUT) (.+) with:")]
    public void WhenIPostWith(string method, string request, string body)
    {
        var router = new SageRouter(CommandsFrom(World), World.Log);
        Parse(request, out var path, out var query);
        _response = router.Handle(method, path, query, body);
    }

    [Then("the response is:")]
    public void ThenTheResponseIs(string expected)
    {
        _response.ShouldNotBeNull("no request ran");
        _response.Status.ShouldBe(200, "server body: " + _response.Body);
        var actual = JToken.Parse(_response.Body);
        var want = JToken.Parse(expected);
        JToken.DeepEquals(actual, want).ShouldBeTrue(
            "GET gave other JSON.\nActual: " + _response.Body + "\nWant: " + want);
    }

    [Then("the response status is {int}")]
    public void ThenTheResponseStatusIs(int status)
    {
        _response.ShouldNotBeNull("no request ran");
        _response.Status.ShouldBe(status, "server body: " + _response.Body);
    }

    [Then("the response reports {string}")]
    public void ThenTheResponseReports(string message)
    {
        _response.ShouldNotBeNull("no request ran");
        var reported = JToken.Parse(_response.Body)["error"];
        reported.ShouldNotBeNull("server body: " + _response.Body);
        reported.ToString().ShouldBe(message, "server body: " + _response.Body);
    }

    [Then("the response contains {string}")]
    public void ThenTheResponseContains(string fragment)
    {
        _response.ShouldNotBeNull("no request ran");
        _response.Status.ShouldBe(200, "server body: " + _response.Body);
        _response.Body.ShouldContain(fragment);
    }

    private static SageCommands CommandsFrom(WorldContext world)
    {
        return new SageCommands(
            world.Items,
            world.Recipes,
            world.Stationpedia,
            world.Player,
            world.Worlds,
            world.IcDevices,
            world.Mips,
            world.Structures,
            world.StructureInspector,
            world.ThingInspector,
            world.Things,
            world.ThingSlots,
            world.RoomInspector,
            world.PipeNetworks);
    }

    private static void Parse(string request, out string path, out IDictionary<string, string> query)
    {
        query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var q = request.IndexOf('?');
        if (q < 0)
        {
            path = request;
            return;
        }

        path = request.Substring(0, q);
        foreach (var pair in request.Substring(q + 1).Split('&'))
        {
            if (string.IsNullOrWhiteSpace(pair))
            {
                continue;
            }

            var eq = pair.IndexOf('=');
            if (eq < 0)
            {
                query[Uri.UnescapeDataString(pair)] = string.Empty;
            }
            else
            {
                query[Uri.UnescapeDataString(pair.Substring(0, eq))] =
                    Uri.UnescapeDataString(pair.Substring(eq + 1));
            }
        }
    }

}
