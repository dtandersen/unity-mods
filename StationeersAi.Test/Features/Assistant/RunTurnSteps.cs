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
    private FakeAgent _agent;

    public RunTurnSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    private FakeAgent Agent => _agent ??= new FakeAgent(World.Events);

    [Given("the agent will answer {string}")]
    public void GivenTheAgentWillAnswer(string answer)
    {
        Agent.AnswersWith(answer);
    }

    [Given("the agent will report the tool call {string}")]
    public void GivenTheAgentWillReportTheToolCall(string call)
    {
        Agent.ReportsToolCall(call);
    }

    [Given("the turn instructions are {string}")]
    public void GivenTheTurnInstructionsAre(string instructions)
    {
        World.SystemPrompt = instructions;
    }

    [Given("the turn allows {int} tool rounds")]
    public void GivenTheTurnAllowsToolRounds(int rounds)
    {
        World.MaxIterations = rounds;
    }

    [When("the turn runs {string}")]
    public async Task WhenTheTurnRuns(string prompt)
    {
        var command = new RunTurnCommand(events => World.GameDataLoaded ? Agent : null, World, World.Log);
        await command.ExecuteAsync(new RunTurnRequest(prompt), World.Conversation, World.Events);
    }

    [Then("the turn sent the instructions {string} and {int} tool rounds")]
    public void ThenTheTurnSentTheInstructionsAndToolRounds(string instructions, int rounds)
    {
        var turn = Agent.Turns.ShouldHaveSingleItem();
        turn.SystemPrompt.ShouldBe(instructions);
        turn.MaxIterations.ShouldBe(rounds);
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
