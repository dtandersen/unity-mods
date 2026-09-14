using System.Collections.Generic;
using System.Threading.Tasks;
using OdinsAi.Test.Support;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Assistant
{
    [Binding]
    public sealed class ToolActivitySteps
    {
        private readonly ScenarioContext _context;

        public ToolActivitySteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [When("I ask the assistant {string} reporting activity")]
        public async Task WhenIAskTheAssistantReportingActivity(string prompt)
        {
            World.Thinking = new List<string>();
            World.ToolOutput = new List<string>();

            var assistant = TestComposition.Assistant(
                World,
                line => World.Thinking.Add(line),
                line => World.ToolOutput.Add(line));

            World.Reply = await assistant.AskAsync(prompt);
        }

        [Then("the thinking reported {string}")]
        public void ThenTheThinkingReported(string expected)
        {
            World.Thinking.ShouldContain(expected);
        }

        [Then("the tool output reported:")]
        public void ThenTheToolOutputReported(string expected)
        {
            World.ToolOutput.ShouldContain(expected.Replace("\r\n", "\n").Trim());
        }
    }
}