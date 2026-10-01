using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using StationeersAi.Command;
using StationeersAi.Test.Support;
using Caev;
using Caev.Tools;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class RunTurnSteps
{
    private readonly ScenarioContext _context;
    private readonly List<string> _think = new List<string>();
    private readonly List<string> _toolCalls = new List<string>();
    private readonly List<string> _toolOutput = new List<string>();
    private readonly List<string> _answered = new List<string>();

    public RunTurnSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [When("the turn runs {string}")]
    public async Task WhenTheTurnRuns(string prompt)
    {
        var command = new RunTurnCommand(
            activity => World.GameDataLoaded ? TestComposition.Assistant(World, activity) : null,
            World.Log);
        command.Think += _think.Add;
        command.ToolCall += _toolCalls.Add;
        command.ToolOutput += _toolOutput.Add;
        command.Answered += _answered.Add;
        await command.ExecuteAsync(new RunTurnRequest(prompt), World.Conversation);
    }

    [Then("the answered event is {string}")]
    public void ThenTheAnsweredEventIs(string expected)
    {
        _answered.ShouldBe(new[] { expected });
    }

    [Then("a tool call event is {string}")]
    public void ThenAToolCallEventIs(string expected)
    {
        _toolCalls.ShouldBe(new[] { expected });
    }
}
