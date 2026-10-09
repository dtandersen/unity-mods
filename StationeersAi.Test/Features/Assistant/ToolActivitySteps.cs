using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class ToolActivitySteps
{
    private readonly ScenarioContext _context;

    public ToolActivitySteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [When("I ask the assistant {string} reporting events")]
    public async Task WhenIAskTheAssistantReportingEvents(string prompt)
    {
        var assistant = TestComposition.Assistant(World, World.Events);
        await assistant.AskAsync(prompt, null);
    }

    [Then("the thinking reported {string}")]
    public void ThenTheThinkingReported(string expected)
    {
        World.Recorder.Reasoning.ShouldContain(expected);
    }

    [Then("the tool calls reported {string}")]
    public void ThenTheToolCallsReported(string expected)
    {
        World.Recorder.ToolCalls.ShouldContain(expected);
    }

    [Then("the tool output reported:")]
    public void ThenTheToolOutputReported(string expected)
    {
        World.Recorder.ToolOutputs.ShouldContain(expected.Replace("\r\n", "\n").Trim());
    }
}
