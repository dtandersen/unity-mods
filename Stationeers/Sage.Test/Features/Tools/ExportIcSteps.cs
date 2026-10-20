using System.Linq;
using Reqnroll;
using Sage.Test.Support;
using Caev.Tools;

namespace Sage.Test.Features.Tools;

[Binding]
public sealed class ExportIcSteps
{
    private readonly ScenarioContext _context;

    public ExportIcSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the export succeeds")]
    public void GivenTheExportSucceeds()
    {
        World.Computer.EditorCode = string.Join("\n", Enumerable.Repeat("yield", 37));
    }

    [Given("the chip reports {string}")]
    public void GivenTheChipReports(string error)
    {
        World.Computer.EditorCode = "yield";
        World.Housing.CompileError = error;
    }

    [Given("the burn does not take")]
    public void GivenTheBurnDoesNotTake()
    {
        World.Computer.EditorCode = "yield";
        World.Computer.BurnTakes = false;
    }

    [When("I export from computer {string} to {string}")]
    public void WhenIExportFromComputerTo(string computer, string device)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "export_ic",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string>
            {
                ["computer"] = computer,
                ["device"] = device,
            })));
    }

    [When("I export from computer {string} without a housing")]
    public void WhenIExportFromComputerWithoutAHousing(string computer)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "export_ic",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string>
            {
                ["computer"] = computer,
            })));
    }

    [Given("the chip of housing {string} holds:")]
    public void GivenTheChipOfHousingHolds(string device, string program)
    {
        World.Housing.Name = device;
        World.Housing.ChipCode = program;
    }
}
