using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Shouldly;
using StationeersAi.Entity;
using StationeersAi.Server;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Server;

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

    [Given("live devices exist:")]
    public void GivenLiveDevicesExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            World.Devices.Devices.Add(new DeviceInfo(
                row["Name"],
                row["Type"],
                long.Parse(row["Thing"]),
                long.Parse(row["Network"]),
                bool.Parse(row["Logic"]),
                long.Parse(row["Room"])));
        }
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
        World.Computer.PointedDevice = device;
        // The same housing "Climate Control" carries thing id 2200 in the
        // circuit-housing routes these features exercise.
        World.Computer.PointedThing = 2200;
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

    private static SageCommands CommandsFrom(WorldContext world)
    {
        return new SageCommands(
            world.Items,
            world.Structures,
            world.Recipes,
            world.Stationpedia,
            world.Player,
            world.Worlds,
            world.IcDevices,
            world.Mips,
            world.Devices,
            world.Inspector,
            world.Things,
            world.ThingSlots,
            world.RoomInspector);
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
