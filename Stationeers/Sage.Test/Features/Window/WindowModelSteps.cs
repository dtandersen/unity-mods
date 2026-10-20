using Reqnroll;
using Shouldly;
using Sage.Test.Support;

namespace Sage.Test.Features.Window;

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
        World.Window = new Sage.Window.WindowModel(count);
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
