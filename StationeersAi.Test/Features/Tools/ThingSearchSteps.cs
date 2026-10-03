using System.Collections.Generic;
using System.Globalization;
using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class ThingSearchSteps
{
    private readonly ScenarioContext _context;

    public ThingSearchSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [Given("these world things exist:")]
    public void GivenTheseWorldThingsExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            World.Things.Things.Add(new ThingInfo(
                row["Name"],
                row["Type"],
                long.Parse(row["Thing"], CultureInfo.InvariantCulture),
                row["Quantity"],
                double.Parse(row["X"], CultureInfo.InvariantCulture),
                double.Parse(row["Y"], CultureInfo.InvariantCulture),
                double.Parse(row["Z"], CultureInfo.InvariantCulture),
                long.Parse(row["Room"], CultureInfo.InvariantCulture)));
        }
    }

    [Given("I face {string}")]
    public void GivenIFace(string degrees)
    {
        var status = World.Player.Status;
        World.Player.Status = new PlayerStatus(
            status != null ? status.X : 0,
            status != null ? status.Y : 0,
            status != null ? status.Z : 0,
            status != null ? status.World : "world",
            null,
            status != null ? status.RoomId : null,
            double.Parse(degrees, CultureInfo.InvariantCulture));
    }

    [When("I call the {string} tool with query {string} from {string} within {string}")]
    public void WhenICallTheToolWithQueryFromWithin(string tool, string query, string origin, string distance)
    {
        var arguments = new Dictionary<string, string>
        {
            ["query"] = query,
            ["origin"] = origin,
            ["distance"] = distance,
        };
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new Caev.Tools.ToolCall("call_1", tool, new Caev.Tools.ToolArguments(arguments)));
    }
}
