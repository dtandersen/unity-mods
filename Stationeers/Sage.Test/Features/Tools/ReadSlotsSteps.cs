using Reqnroll;
using Sage.Test.Support;
using Caev.Tools;

namespace Sage.Test.Features.Tools;

[Binding]
public sealed class ReadSlotsSteps
{
    private readonly ScenarioContext _context;

    public ReadSlotsSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("nothing is in any slot")]
    public void GivenNothingIsInAnySlot()
    {
        World.ThingSlots.Contents.Clear();
    }

    [Given("no thing holds a match")]
    public void GivenNoThingHoldsAMatch()
    {
        World.ThingSlots.HasMatch = false;
    }

    [When("I read slots of thing {string}")]
    public void WhenIReadSlotsOfThing(string thing)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "read_slots",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string>
            {
                ["thing"] = thing,
            })));
    }
}
