using Reqnroll;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class ImportIcSteps
{
    private readonly ScenarioContext _context;

    public ImportIcSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [When("I import from housing {string} into {string}")]
    public void WhenIImportFromHousingInto(string device, string computer)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "import_ic",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string>
            {
                ["computer"] = computer,
                ["device"] = device,
            })));
    }
}
