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

        [When("I write to computer {string}:")]
        public void WhenIWriteToComputer(string computer, string code)
        {
            Write(new Dictionary<string, string>
            {
                ["computer_name"] = computer,
                ["code"] = code,
            });
        }

        [When("I write to thing {string}:")]
        public void WhenIWriteToThing(string thing, string code)
        {
            Write(new Dictionary<string, string>
            {
                ["computer_thing"] = thing,
                ["code"] = code,
            });
        }

        private void Write(Dictionary<string, string> values)
        {
            values["code"] = values["code"].Replace("\r\n", "\n").Trim();
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "write_ic",
                new ToolArguments(values)));
        }
    }
}
