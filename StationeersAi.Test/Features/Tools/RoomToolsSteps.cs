using System.Collections.Generic;
using System.Globalization;
using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class RoomToolsSteps
{
    private readonly ScenarioContext _context;

    public RoomToolsSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [Given("a {word} room {int} at {word} C and {word} kPa in {int} L holding:")]
    public void GivenARoomHolding(string type, int room, string temperature, string pressure, int volume, Table gases)
    {
        var amounts = new List<GasMoles>();
        foreach (var row in gases.Rows)
        {
            amounts.Add(new GasMoles(
                row["Gas"],
                double.Parse(row["Moles"], CultureInfo.InvariantCulture)));
        }

        World.RoomInspector.Add(new RoomAtmosphere(
            room,
            type,
            double.Parse(temperature, CultureInfo.InvariantCulture),
            double.Parse(pressure, CultureInfo.InvariantCulture),
            volume,
            amounts));
    }

    [Given("I stand in room {int}")]
    public void GivenIStandInRoom(int room)
    {
        World.Player.Status = new PlayerStatus(0, 0, 0, "world", null, room, null);
    }

    [Given("I stand outside")]
    public void GivenIStandOutside()
    {
        World.Player.Status = new PlayerStatus(0, 0, 0, "world", null, null, null);
    }

    [When("I call the {string} tool with room {string}")]
    public void WhenICallTheToolWithRoom(string tool, string room)
    {
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new ToolCall("call_1", tool, new ToolArguments(
                new Dictionary<string, string> { ["room"] = room })));
    }
}
