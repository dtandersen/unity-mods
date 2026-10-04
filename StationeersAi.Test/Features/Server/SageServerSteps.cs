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

    private World World => Hooks.GetWorld(_context);

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
        World.Mips.Instructions.Add(new MipsInstruction(first, $"{first} r? device var", $"Does {first}."));
        World.Mips.Instructions.Add(new MipsInstruction(second, second, $"Does {second}."));
    }

    [Given("the IC editor holds {string} on {string}")]
    public void GivenTheIcEditorHolds(string code, string computer)
    {
        World.Ic.ResolveAs = computer;
        World.Ic.EditorCode = code;
    }

    [When(@"I GET (.+)")]
    public void WhenIGet(string request)
    {
        var router = new SageRouter(SageDataFrom(World));
        Parse(request, out var path, out var query);
        _response = router.Handle("GET", path, query);
    }

    [When(@"I POST (.+) with:")]
    public void WhenIPostWith(string request, string body)
    {
        var router = new SageRouter(SageDataFrom(World));
        Parse(request, out var path, out var query);
        _response = router.Handle("POST", path, query, body);
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

    private static SageData SageDataFrom(World world)
    {
        return new SageData
        {
            Items = world.Items,
            Structures = world.Structures,
            Recipes = world.Recipes,
            Stationpedia = world.Stationpedia,
            Player = world.Player,
            Worlds = world.Worlds,
            Ic = world.Ic,
            Export = world.Export,
            Mips = world.Mips,
            Devices = world.Devices,
            Inspector = world.Inspector,
            Things = world.Things,
            ThingContents = world.ThingContents,
            Rooms = world.RoomInspector,
        };
    }
}
