using Reqnroll;
using System.Collections.Generic;
using Sage.Entity;
using Sage.Test.Support;
using Caev.Tools;
using System;
using System.Globalization;
using System.Linq;

namespace Sage.Test.Features.Tools;

[Binding]
public sealed class DescribeStructureSteps
{
    private readonly ScenarioContext _context;

    public DescribeStructureSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the structure {string} exists")]
    public void GivenTheStructureExists(string name)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "StructureAirVent",
            101,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 7) },
            true,
            new[] { new StructureLogicValue("Pressure", 101.3, false), new StructureLogicValue("On", 1, true) },
            new[] { new StructureSlotInfo(0, "Filter", "empty") },
            null,
            null,
            null,
            null);
    }

    [Given("the structure {string} holds:")]
    public void GivenTheStructureHolds(string name, Table table)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "StructureAutolathe",
            1456,
            new List<NetworkConnection>(),
            false,
            new StructureLogicValue[0],
            new[]
            {
                new StructureSlotInfo(0, "Import", "empty"),
                new StructureSlotInfo(1, "Export", "empty"),
            },
            null,
            null,
            table.Rows.Select(row => new ReagentAmount(
                row["Reagent"],
                double.Parse(row["Quantity"], CultureInfo.InvariantCulture))).ToArray(),
            null);
    }

    [Given("the housing {string} exists")]
    public void GivenTheHousingExists(string name)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "StructureCircuitHousing",
            100,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 7) },
            null,
            new StructureLogicValue[0],
            new StructureSlotInfo[0],
            new[]
            {
                new HousingDeviceEntry("db", name, 100),
                new HousingDeviceEntry("d0", "Heater", 101),
                new HousingDeviceEntry("d1", "empty", 0),
            },
            null,
            null,
            null);
    }

    [Given("the housing {string} runs code")]
    public void GivenTheHousingRunsCode(string name)
    {
        GivenTheHousingExists(name);
        var details = World.StructureInspector.Details;
        World.StructureInspector.Details = new StructureDetails(
            details.Name,
            details.PrefabName,
            details.Id,
            details.Connections,
            details.IsOn,
            details.Logic,
            details.Slots,
            details.Devices,
            new ChipState("IncorrectVariable", "12", 294, "6c96f2ae"),
            details.Storage,
            details.Build);
    }

    [Given("the structure {string} stands at stage {int} of {int} with steps:")]
    public void GivenTheStructureStandsAtStage(string name, int stage, int stages, Table table)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "Structure" + name.Replace(" ", string.Empty),
            101,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 7) },
            true,
            new StructureLogicValue[0],
            new StructureSlotInfo[0],
            null,
            null,
            null,
            new BuildProgress(
                stage,
                stages,
                DatabaseSteps.Steps(table)));
    }

    [Given("the structure {string} stands finished")]
    public void GivenTheStructureStandsFinished(string name)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "Structure" + name.Replace(" ", string.Empty),
            101,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 7) },
            true,
            new StructureLogicValue[0],
            new StructureSlotInfo[0],
            null,
            null,
            null,
            new BuildProgress(1, 1, Array.Empty<BuildStep>()));
    }

    [Given("no structure matches")]
    public void GivenNoStructureMatches()
    {
        World.StructureInspector.Details = null;
    }

    [Given("the gas sensor {string} reads 290K at 3.3 kPa")]
    public void GivenTheGasSensorReads(string name)
    {
        World.StructureInspector.Details = new StructureDetails(
            name,
            "StructureGasSensor",
            1482,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 1194) },
            null,
            new[]
            {
                new StructureLogicValue("Pressure", 3.35518, false),
                new StructureLogicValue("Temperature", 290.131, false),
                new StructureLogicValue("RatioOxygen", 0.195957, false),
                new StructureLogicValue("RatioCarbonDioxide", 0.774701, false),
                new StructureLogicValue("RatioMethane", 0, false),
                new StructureLogicValue("RatioWater", 0, false),
                new StructureLogicValue("RatioNitrogen", 0.0241535, false),
            },
            new StructureSlotInfo[0],
            null,
            null,
            null,
            null);
    }

    [Given("two structures share the name {string}")]
    public void GivenTwoStructuresShareTheName(string name)
    {
        World.Structures.Structures.Clear();
        World.Structures.Structures.Add(new StructureInfo(name, "StructureGasSensor", 1482, new List<NetworkConnection> { new NetworkConnection("power", 0, 1194) }, true, 5, new StructurePosition(1, 2, 3)));
        World.Structures.Structures.Add(new StructureInfo(name, "StructureGasSensor", 1123, new List<NetworkConnection> { new NetworkConnection("power", 0, 1119) }, true, 5, new StructurePosition(4, 5, 6)));
        World.StructureInspector.Details = null;
    }

    [When("I describe the structure {string} with full {string}")]
    public void WhenIDescribeTheStructureWithFull(string structure, string full)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "describe_structure",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string> { ["structure"] = structure, ["full"] = full })));
    }

    [When("I describe the structure {string}")]
    public void WhenIDescribeTheStructure(string structure)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "describe_structure",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string> { ["structure"] = structure })));
    }
}
