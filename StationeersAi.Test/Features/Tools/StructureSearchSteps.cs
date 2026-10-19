using System.Collections.Generic;
using Reqnroll;
using Shouldly;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using Caev.Tools;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class StructureSearchSteps
{
    private readonly ScenarioContext _context;

    public StructureSearchSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the world has structures")]
    public void GivenTheWorldHasStructures()
    {
        World.Structures.Structures.Add(new StructureInfo("Air Vent", "StructureAirVent", 101, new List<NetworkConnection> { new NetworkConnection("power", 0, 7) }, true, 5, new StructurePosition(1, 2, 3)));
        World.Structures.Structures.Add(new StructureInfo("Gas Sensor", "StructureGasSensor", 102, new List<NetworkConnection>(), true, 5, new StructurePosition(4, 5, 6)));
        World.Structures.Structures.Add(new StructureInfo("Iron Pipe", "StructurePipe", 103, new List<NetworkConnection>(), false, 0, new StructurePosition(7, 8, 9)));
    }

    [Given("the world has 12 structures")]
    public void GivenTheWorldHas12Structures()
    {
        World.Structures.Structures.Clear();
        for (var i = 1; i <= 12; i++)
        {
            World.Structures.Structures.Add(new StructureInfo($"Structure {i:00}", "StructurePipe", 100 + i, new List<NetworkConnection>(), false, 0, new StructurePosition(i, 0, 0)));
        }
    }

    [Given("the world has cables plus a heater")]
    public void GivenTheWorldHasCablesPlusHeater()
    {
        World.Structures.Structures.Clear();
        for (var i = 1; i <= 11; i++)
        {
            World.Structures.Structures.Add(new StructureInfo($"Cable {i:00}", "StructureCable", 200 + i, new List<NetworkConnection>(), false, 0, new StructurePosition(i, 0, 0)));
        }

        World.Structures.Structures.Add(new StructureInfo("Wall Heater", "StructureWallHeater", 1208, new List<NetworkConnection> { new NetworkConnection("power", 0, 1194) }, true, 5, new StructurePosition(1, 2, 3)));
    }

    [Given("the world has 12 logic structures")]
    public void GivenTheWorldHas12LogicStructures()
    {
        World.Structures.Structures.Clear();
        for (var i = 1; i <= 12; i++)
        {
            World.Structures.Structures.Add(new StructureInfo($"Sensor {i:00}", "StructureGasSensor", 1400 + i, new List<NetworkConnection> { new NetworkConnection("power", 0, 1194) }, true, 5, new StructurePosition(i, 0, 0)));
        }
    }

    [Given("the world has a battery and a furnace")]
    public void GivenTheWorldHasABatteryAndFurnace()
    {
        World.Structures.Structures.Clear();
        World.Structures.Structures.Add(new StructureInfo("Station Battery", "StructureBattery", 10040, new List<NetworkConnection>
        {
            new NetworkConnection("power", 0, 29457),
            new NetworkConnection("power", 1, 29457),
            new NetworkConnection("data", 0, 7),
        }, true, 2, new StructurePosition(1, 2, 3)));
        World.Structures.Structures.Add(new StructureInfo("Furnace", "StructureFurnace", 5229, new List<NetworkConnection>
        {
            new NetworkConnection("pipe", 0, 11, "input"),
            new NetworkConnection("pipe", 2, 22, "output"),
            new NetworkConnection("pipe", 3, 33, "output2"),
        }, false, 0, new StructurePosition(4, 5, 6)));
    }

    [When("I search structures on cable network {string}")]
    [When("I search structures on network {string}")]
    public void WhenISearchStructuresOnNetwork(string network)
    {
        Search(network, null);
    }

    [When("I search structures for {string} on cable network {string}")]
    [When("I search structures for {string} on network {string}")]
    public void WhenISearchStructuresForOnNetwork(string query, string network)
    {
        Search(network, query);
    }

    [Then("the search used cable network {string}")]
    [Then("the search used network {string}")]
    public void ThenTheSearchUsedNetwork(string expected)
    {
        World.Structures.LastPowerNetworkId.ShouldBe(long.Parse(expected));
    }
    [When("I search structures with logic {string}")]
    public void WhenISearchStructuresWithLogic(string logic)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "search_structures",
            new ToolArguments(new Dictionary<string, string> { ["logic"] = logic })));
    }

    private void Search(string network, string query)
    {
        var values = new Dictionary<string, string>();
        if (network != null)
        {
            values["powerNetworkId"] = network;
        }

        if (query != null)
        {
            values["query"] = query;
        }

        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "search_structures",
            new ToolArguments(values)));
    }
    [When("I search structures with offset {string}")]
    public void WhenISearchStructuresWithOffset(string offset)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "search_structures",
            new ToolArguments(new Dictionary<string, string> { ["offset"] = offset })));
    }

    [Then("the tool found {int} structures")]
    public void ThenTheToolFoundStructures(int count)
    {
        World.ToolResult.ShouldBeOfType<ToolSuccess<SearchStructuresResult>>().Value.Structures.Count.ShouldBe(count);
    }
    [When("I search structures in room {string}")]
    public void WhenISearchStructuresInRoom(string room)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "search_structures",
            new ToolArguments(new Dictionary<string, string> { ["room"] = room })));
    }

    [Then("the search used room {string}")]
    public void ThenTheSearchUsedRoom(string expected)
    {
        World.Structures.LastRoom.ShouldBe(long.Parse(expected));
    }
}
