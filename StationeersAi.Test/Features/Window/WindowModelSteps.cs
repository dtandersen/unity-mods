using Reqnroll;
using Shouldly;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Window;

[Binding]
public sealed class WindowModelSteps
{
    private readonly ScenarioContext _context;

    public WindowModelSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the window history holds {int} lines")]
    public void GivenTheWindowHistoryHolds(int count)
    {
        World.Window = new StationeersAi.Window.WindowModel(count);
    }

    [Given("the model received the line {string}")]
    [When("the model receives the line {string}")]
    public void TheModelReceivesTheLine(string line)
    {
        World.Window.Append(line);
    }

    [When("the history is cleared")]
    public void WhenTheHistoryIsCleared()
    {
        World.Window.Clear();
    }

    [When("the thinking status is {string}")]
    public void WhenTheThinkingStatusIs(string line)
    {
        World.Window.SetThinking(line);
    }

    [When("thinking is cleared")]
    public void WhenThinkingIsCleared()
    {
        World.Window.ClearThinking();
    }

    [Then("the window history is:")]
    public void ThenTheWindowHistoryIs(string expected)
    {
        string.Join("\n", World.Window.Lines).ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the window history is empty")]
    public void ThenTheWindowHistoryIsEmpty()
    {
        World.Window.Lines.ShouldBeEmpty();
    }
}
