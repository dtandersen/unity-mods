using System.Collections.Generic;
using Reqnroll;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class WriteIcSteps
{
    private readonly ScenarioContext _context;

    public WriteIcSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("a computer {string}")]
    public void GivenAComputer(string name)
    {
        World.Computer.Name = name;
    }

    [Given("no computer is found")]
    public void GivenNoComputerIsFound()
    {
        World.Computers.Found = false;
    }

    [When("I write to computer {string}:")]
    public void WhenIWriteToComputer(string computer, string code)
    {
        var values = new Dictionary<string, string>
        {
            ["computer"] = computer,
            ["code"] = code.Replace("\r\n", "\n").Trim(),
        };
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "write_ic",
            new ToolArguments(values)));
    }
}
