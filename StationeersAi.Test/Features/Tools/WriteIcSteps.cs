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
        World.IcDevices.ComputerFound = false;
    }

    [Given("the chip under the dropdown holds {string}")]
    public void GivenTheChipUnderTheDropdownHolds(string code)
    {
        World.Computer.SelectedDevice = "Climate Control";
        World.Housing.ChipCode = code;
    }

    [Given("the computer points at nothing")]
    public void GivenTheComputerPointsAtNothing()
    {
        World.Computer.SelectedDevice = null;
    }

    [When("I write to computer {string} with chip hash {string}:")]
    public void WhenIWriteToComputerWithChipHash(string computer, string hash, string code)
    {
        Write(computer, code, hash);
    }

    [When("I write to computer {string} without a chip hash:")]
    public void WhenIWriteToComputerWithoutAChipHash(string computer, string code)
    {
        Write(computer, code, "");
    }

    private void Write(string computer, string code, string hash)
    {
        var values = new Dictionary<string, string>
        {
            ["computer"] = computer,
            ["code"] = code.Replace("\r\n", "\n").Trim(),
        };

        if (!string.IsNullOrEmpty(hash))
        {
            values["hash"] = hash;
        }

        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "write_ic",
            new ToolArguments(values)));
    }
}
