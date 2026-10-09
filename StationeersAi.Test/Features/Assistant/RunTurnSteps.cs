using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using Caev;
using StationeersAi.Command;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class RunTurnSteps
{
    private readonly ScenarioContext _context;
    private FakeAiAssistant _assistant;

    public RunTurnSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    private FakeAiAssistant Assistant => _assistant ??= new FakeAiAssistant(World.Events);

    [Given("the assistant will answer {string}")]
    public void GivenTheAssistantWillAnswer(string answer)
    {
        Assistant.AnswersWith(answer);
    }

    [Given("the assistant will report the tool call {string}")]
    public void GivenTheAssistantWillReportTheToolCall(string call)
    {
        Assistant.ReportsToolCall(call);
    }

    [When("the turn runs {string}")]
    public async Task WhenTheTurnRuns(string prompt)
    {
        var command = new RunTurnCommand(events => World.GameDataLoaded ? Assistant : null, World.Log);
        await command.ExecuteAsync(new RunTurnRequest(prompt), World.Conversation, World.Events);
    }

    [Then("the answered event is {string}")]
    public void ThenTheAnsweredEventIs(string expected)
    {
        World.Recorder.Answers.ShouldBe(new[] { expected });
    }

    [Then("a tool call event is {string}")]
    public void ThenAToolCallEventIs(string expected)
    {
        World.Recorder.ToolCalls.ShouldBe(new[] { expected });
    }
}
