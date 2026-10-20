using System.Threading.Tasks;
using OdinsAi.Test.Fakes;
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
            World.Activity = new RecordingActivity();
            var assistant = TestComposition.Assistant(World, World.Activity);
            World.Reply = await assistant.AskAsync(prompt);
        }

        [Then("the thinking reported {string}")]
        public void ThenTheThinkingReported(string expected)
        {
            World.Activity.Reasoning.ShouldContain(expected);
        }

        [Then("the tool calls reported {string}")]
        public void ThenTheToolCallsReported(string expected)
        {
            World.Activity.ToolCalls.ShouldContain(expected);
        }

        [Then("the tool output reported:")]
        public void ThenTheToolOutputReported(string expected)
        {
            World.Activity.ToolOutput.ShouldContain(expected.Replace("\r\n", "\n").Trim());
        }
    }
}