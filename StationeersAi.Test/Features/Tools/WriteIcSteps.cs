using System.Collections.Generic;
using Reqnroll;
using StationeersAi.Test.Support;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools
{
    [Binding]
    public sealed class WriteIcSteps
    {
        private readonly ScenarioContext _context;

        public WriteIcSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("a computer {string}")]
        public void GivenAComputer(string name)
        {
            World.Ic.ResolveAs = name;
        }

        [Given("no computer is found")]
        public void GivenNoComputerIsFound()
        {
            World.Ic.ResolveAs = null;
        }

        [When("I write to device {string}:")]
        public void WhenIWriteToDevice(string device, string code)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "write_ic",
                new ToolArguments(new Dictionary<string, string>
                {
                    ["device"] = device,
                    ["code"] = code.Replace("\r\n", "\n").Trim(),
                })));
        }
    }
}
