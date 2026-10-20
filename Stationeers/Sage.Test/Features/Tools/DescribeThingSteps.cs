using Reqnroll;
using Sage.Entity;
using Sage.Test.Support;
using Caev.Tools;
using System.Collections.Generic;

namespace Sage.Test.Features.Tools;

[Binding]
public sealed class DescribeThingSteps
{
    private readonly ScenarioContext _context;

    public DescribeThingSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the structure thing {string} exists")]
    public void GivenTheStructureThingExists(string name)
    {
        World.ThingInspector.Details = new ThingDetails(
            name,
            "StructureAirVent",
            101,
            "structure",
            null,
            new StructurePosition(1, 2, 3),
            5,
            new List<NetworkConnection> { new NetworkConnection("power", 0, 7) },
            true,
            new StructureDetails(
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
                null));
    }

    [Given("the loose thing {string} exists")]
    public void GivenTheLooseThingExists(string name)
    {
        World.ThingInspector.Details = new ThingDetails(
            name,
            "OreIron",
            201,
            "thing",
            "x5",
            new StructurePosition(4, 5, 6),
            0,
            new List<NetworkConnection>(),
            false,
            null);
    }

    [Given("no thing matches")]
    public void GivenNoThingMatches()
    {
        World.ThingInspector.Details = null;
        World.Structures.Structures.Clear();
        World.Things.Things.Clear();
    }

    [When("I describe the thing {string}")]
    public void WhenIDescribeTheThing(string thing)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "describe_thing",
            new ToolArguments(new Dictionary<string, string> { ["thing"] = thing })));
    }
}
