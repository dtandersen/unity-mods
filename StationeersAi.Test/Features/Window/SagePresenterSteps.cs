using System;
using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;
using StationeersAi.Window;

namespace StationeersAi.Test.Features.Window;

[Binding]
public sealed class SagePresenterSteps
{
    private readonly ScenarioContext _context;

    public SagePresenterSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    private SagePresenter EnsurePresenter()
    {
        if (World.WindowPresenter != null)
        {
            return World.WindowPresenter;
        }

        World.WindowView = new FakeSageView();
        World.WindowPresenter = new SagePresenter(
            World.Window,
            World.WindowView,
            Ask);
        return World.WindowPresenter;
    }

    private Task Ask(string prompt)
    {
        World.AskedPrompts.Add(prompt);
        World.PendingAnswer ??= new TaskCompletionSource<object>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        return World.PendingAnswer.Task;
    }

    [Given("the window input is {string}")]
    public void GivenTheWindowInputIs(string input)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.WindowView.Input = input;
    }

    [Given("the view history holds {int} lines")]
    public void GivenTheViewHistoryHolds(int count)
    {
        World.Window = new WindowModel(count);
        World.WindowView = new FakeSageView();
        World.WindowPresenter = new SagePresenter(
            World.Window,
            World.WindowView,
            Ask);
    }

    [Given("the assistant is still answering {string}")]
    public void GivenTheAssistantIsStillAnswering(string question)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.WindowView.Input = question;
        World.WindowView.RaiseSubmit();
        World.AskedPrompts.ShouldBe(new[] { question });
    }

    [When("the user submits")]
    public void WhenTheUserSubmits()
    {
        World.WindowView.RaiseSubmit();
    }

    [When("the console asks {string}")]
    public void WhenTheConsoleAsks(string question)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.AskTask = World.WindowPresenter.RunTurnAsync(question, Ask);
    }

    [When("the user toggles the window")]
    public void WhenTheUserTogglesTheWindow()
    {
        EnsurePresenter().ShouldNotBeNull();
        World.WindowView.RaiseToggle();
    }

    [When("the presenter receives the line {string}")]
    public void WhenThePresenterReceivesTheLine(string line)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.WindowPresenter.Write(line);
    }

    [When("the answer finishes")]
    public async Task WhenTheAnswerFinishes()
    {
        World.PendingAnswer.ShouldNotBeNull();
        World.PendingAnswer.TrySetResult(null);
        var deadline = Environment.TickCount64 + 5000;
        while (World.WindowPresenter.Busy && Environment.TickCount64 < deadline)
        {
            await Task.Delay(10);
        }
    }

    [Then("the asked question is {string}")]
    public void ThenTheAskedQuestionIs(string expected)
    {
        World.AskedPrompts.Count.ShouldBe(1);
        World.AskedPrompts[0].ShouldBe(expected);
    }

    [Then("nothing is asked")]
    public void ThenNothingIsAsked()
    {
        World.AskedPrompts.ShouldBeEmpty();
    }

    [Then("the window input is {string}")]
    public void ThenTheWindowInputIs(string expected)
    {
        World.WindowView.Input.ShouldBe(expected);
    }

    [Then("the view shows:")]
    public void ThenTheViewShows(string expected)
    {
        string.Join("\n", World.WindowView.Lines).ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the window is busy")]
    public void ThenTheWindowIsBusy()
    {
        World.WindowPresenter.Busy.ShouldBeTrue();
    }

    [Then("the window is not busy")]
    public void ThenTheWindowIsNotBusy()
    {
        World.WindowPresenter.Busy.ShouldBeFalse();
    }

    [Then("the view is busy")]
    public void ThenTheViewIsBusy()
    {
        World.WindowView.Busy.ShouldBeTrue();
    }

    [Then("the view is not busy")]
    public void ThenTheViewIsNotBusy()
    {
        World.WindowView.Busy.ShouldBeFalse();
    }

    [Then("the window is visible")]
    public void ThenTheWindowIsVisible()
    {
        World.WindowPresenter.Visible.ShouldBeTrue();
        World.WindowView.Visible.ShouldBeTrue();
    }

    [Then("the window is hidden")]
    public void ThenTheWindowIsHidden()
    {
        World.WindowPresenter.Visible.ShouldBeFalse();
        World.WindowView.Visible.ShouldBeFalse();
    }

    [Then("the input is focused")]
    public void ThenTheInputIsFocused()
    {
        World.WindowView.FocusRequests.ShouldBeGreaterThan(0);
    }
}
