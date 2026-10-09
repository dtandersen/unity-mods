using System;
using System.Linq;
using System.Threading.Tasks;
using Caev;
using Reqnroll;
using Shouldly;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;
using StationeersAi.Presentation;
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

    private WorldContext World => Hooks.GetWorld(_context);

    private SagePresenter EnsurePresenter()
    {
        if (World.Presenter != null)
        {
            return World.Presenter;
        }

        World.Presenter = BuildPresenter();
        return World.Presenter;
    }

    private static readonly DateTimeOffset TurnStarted = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private bool _clockArmed = true;

    private SagePresenter BuildPresenter()
    {
        return TestComposition.Presenter(World, clock: () =>
            {
                // Scripted time stands still, so trip it once per turn: the
                // start reads the base instant, progress reads it elapsed.
                if (_clockArmed)
                {
                    _clockArmed = false;
                    return TurnStarted;
                }

                return TurnStarted + World.ThinkingElapsed;
            });
    }

    private string[] AskedQuestions()
    {
        return World.Ai.Transcript
                .SelectMany(messages => messages)
                .Where(message => message.Role == ChatRole.User)
                .Select(message => message.Content)
                .ToArray();
    }

    /// <summary>
    /// Runs the console line through the scenario presenter, the same path
    /// the game's <c>ai</c> command takes. The shell splits the raw line on
    /// spaces, so the step does the same after stripping the <c>ai</c> prefix.
    /// </summary>
    private string ConsoleCommand(string command)
    {
        return World.Presenter.OnConsoleCommand(SplitConsoleLine(command));
    }

    private static string[] SplitConsoleLine(string command)
    {
        var line = (command ?? string.Empty).Trim();
        if (line.StartsWith("ai ", StringComparison.OrdinalIgnoreCase))
        {
            line = line.Substring(3);
        }
        else if (line.Equals("ai", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<string>();
        }

        return line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
        World.Presenter = BuildPresenter();
    }

    [Given("the assistant is still answering {string}")]
    public async Task GivenTheAssistantIsStillAnswering(string question)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.Ai.Hold = true;
        World.WindowView.Input = question;
        World.WindowView.RaiseSubmit();
        World.Presenter.Busy.ShouldBeTrue();
        await WaitUntil(() => AskedQuestions().Contains(question));
    }

    [Given("{int} seconds have passed")]
    public void GivenSecondsHavePassed(int seconds)
    {
        World.ThinkingElapsed = TimeSpan.FromSeconds(seconds);
    }

    [Given("the AI model will pause after {int} calls")]
    public void GivenTheAiModelWillPauseAfterCalls(int count)
    {
        World.Ai.PauseAfterCalls = count;
    }

    [Given("the SAGE window isn't visible")]
    public void GivenTheSageWindowIsntVisible()
    {
        EnsurePresenter().ShouldNotBeNull();
        World.Presenter.Visible.ShouldBeFalse();
    }

    [Given("the AI agent has context")]
    public void GivenTheAiAgentHasContext()
    {
        World.Conversation.Add(new ChatMessage(ChatRole.User, "what time is it?"));
        World.Conversation.Add(new ChatMessage(ChatRole.Assistant, "it is noon"));
    }

    [When("the turn pauses")]
    public async Task WhenTheTurnPauses()
    {
        await WaitUntil(() => World.WindowView.Lines.Any(line => line.Contains("Thinking")));
    }

    [When("the user submits")]
    public void WhenTheUserSubmits()
    {
        _clockArmed = true;
        World.WindowView.RaiseSubmit();
    }

    [When("the console asks {string}")]
    public void WhenTheConsoleAsks(string question)
    {
        EnsurePresenter().ShouldNotBeNull();
        _clockArmed = true;
        World.AskTask = World.Presenter.AskAsync(question);
    }

    [When("the user types the command {string}")]
    public void WhenTheUserTypesTheCommand(string command)
    {
        EnsurePresenter().ShouldNotBeNull();
        _clockArmed = true;
        World.ConsoleOutput = ConsoleCommand(command);
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
        World.Presenter.Write(line);
    }

    [When("the turn completes")]
    public async Task WhenTheTurnCompletes()
    {
        await WaitUntil(() => !World.Presenter.Busy);
    }

    [When("the answer finishes")]
    public async Task WhenTheAnswerFinishes()
    {
        World.Ai.ShouldNotBeNull();
        World.Ai.Release();
        await WaitUntil(() => !World.Presenter.Busy);
    }

    [When("the user clears the context")]
    public void WhenTheUserClearsTheContext()
    {
        EnsurePresenter().ShouldNotBeNull();
        World.Presenter.ClearConversation();
    }

    [When("the user compacts the context")]
    public async Task WhenTheUserCompactsTheContext()
    {
        EnsurePresenter().ShouldNotBeNull();
        await World.Presenter.CompactConversation();
    }

    [Then("the asked question is {string}")]
    public void ThenTheAskedQuestionIs(string expected)
    {
        AskedQuestions().ShouldBe(new[] { expected });
    }

    [Then("the question {string} is queued")]
    public async Task ThenTheQuestionIsQueued(string expected)
    {
        await WaitUntil(() => AskedQuestions().Contains(expected));
    }

    [Then("the console displays:")]
    public void ThenTheConsoleDisplays(string expected)
    {
        (World.ConsoleOutput ?? string.Empty).ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }

    [Then("the AI agent has an empty context")]
    public void ThenTheAiAgentHasAnEmptyContext()
    {
        World.Conversation.Messages.ShouldBeEmpty();
    }

    [Then("the AI agent compacts its context")]
    public async Task ThenTheAiAgentCompactsItsContext()
    {
        await WaitUntil(() => World.Conversation.Messages.Count == 1);
        var summary = World.Conversation.Messages.ShouldHaveSingleItem();
        summary.Role.ShouldBe(ChatRole.Assistant);
    }

    [Then("the SAGE window is visible")]
    public void ThenTheSageWindowIsVisible()
    {
        World.Presenter.Visible.ShouldBeTrue();
        World.WindowView.Visible.ShouldBeTrue();
    }

    [Then("nothing is asked")]
    public void ThenNothingIsAsked()
    {
        AskedQuestions().ShouldBeEmpty();
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
        World.Presenter.Busy.ShouldBeTrue();
    }

    [Then("the window is not busy")]
    public void ThenTheWindowIsNotBusy()
    {
        World.Presenter.Busy.ShouldBeFalse();
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
        World.Presenter.Visible.ShouldBeTrue();
        World.WindowView.Visible.ShouldBeTrue();
    }

    [Then("the window is hidden")]
    public void ThenTheWindowIsHidden()
    {
        World.Presenter.Visible.ShouldBeFalse();
        World.WindowView.Visible.ShouldBeFalse();
    }

    [Then("the input is focused")]
    public void ThenTheInputIsFocused()
    {
        World.WindowView.FocusRequests.ShouldBeGreaterThan(0);
    }

    [Then("the input is not focused")]
    public void ThenTheInputIsNotFocused()
    {
        World.WindowView.FocusRequests.ShouldBe(0);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = Environment.TickCount64 + 5000;
        while (!condition() && Environment.TickCount64 < deadline)
        {
            await Task.Delay(10);
        }

        condition().ShouldBeTrue("timed out waiting for the window");
    }
}
