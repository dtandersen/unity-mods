using System.Collections.Generic;
using Reqnroll;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class ReadIcSteps
{
    private readonly ScenarioContext _context;

    public ReadIcSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [Given("the editor holds {string}")]
    public void GivenTheEditorHolds(string code)
    {
        World.Ic.EditorCode = code;
    }

    [Given("the editor is empty")]
    public void GivenTheEditorIsEmpty()
    {
        World.Ic.EditorCode = string.Empty;
    }

    [Given("no housing is found")]
    public void GivenNoHousingIsFound()
    {
        World.Housing.ResolveAs = null;
    }

    [When("I read computer {string}")]
    public void WhenIReadComputer(string computer)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "read_ic",
            new ToolArguments(new Dictionary<string, string> { ["computer"] = computer })));
    }

    [When("I read computer {string} housing {string}")]
    public void WhenIReadComputerHousing(string computer, string device)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "read_ic",
            new ToolArguments(new Dictionary<string, string>
            {
                ["computer"] = computer,
                ["device"] = device,
            })));
    }
}
