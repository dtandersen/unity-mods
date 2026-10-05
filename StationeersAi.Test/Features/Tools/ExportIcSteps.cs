using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class ExportIcSteps
{
    private readonly ScenarioContext _context;

    public ExportIcSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [Given("the export succeeds")]
    public void GivenTheExportSucceeds()
    {
        World.Export.Result = new IcWriteResult(true, "Climate Control", 37, null);
    }

    [Given("the export fails with {string}")]
    public void GivenTheExportFailsWith(string error)
    {
        World.Export.Result = new IcWriteResult(false, null, 0, error);
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

    [Given("the import succeeds")]
    public void GivenTheImportSucceeds()
    {
        World.Export.ImportResult = new IcWriteResult(true, "Lab Computer", 3, null);
    }

    [Given("the import fails with {string}")]
    public void GivenTheImportFailsWith(string error)
    {
        World.Export.ImportResult = new IcWriteResult(false, null, 0, error);
    }

    [When("I import from housing {string} into {string}")]
    public void WhenIImportFromHousingInto(string device, string computer)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "export_ic",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string>
            {
                ["computer"] = computer,
                ["device"] = device,
                ["import"] = "true",
            })));
    }
}
