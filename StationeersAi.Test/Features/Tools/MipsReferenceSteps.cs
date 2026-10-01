using System.Collections.Generic;
using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools
{
    [Binding]
    public sealed class MipsReferenceSteps
    {
        private readonly ScenarioContext _context;

        public MipsReferenceSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("the MIPS reference knows {string} and {string}")]
        public void GivenTheMipsReferenceKnows(string first, string second)
        {
            World.Mips.Instructions.Add(new MipsInstruction(first, $"{first} r? device var", $"Does {first}."));
            World.Mips.Instructions.Add(new MipsInstruction(second, second, $"Does {second}."));
        }

        [When("I describe the instruction {string}")]
        public void WhenIDescribeTheInstruction(string instruction)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "mips_reference",
                new ToolArguments(new Dictionary<string, string> { ["instruction"] = instruction })));
        }
    }
}
