using System;
using System.Linq;
using System.Threading.Tasks;
using Caev;
using Reqnroll;
using Shouldly;
using StationeersAi.Command;
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
        World.WindowPresenter = BuildPresenter(World.Window);
        return World.WindowPresenter;
    }

    private static readonly DateTimeOffset TurnStarted = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private bool _clockArmed = true;

    private SagePresenter BuildPresenter(WindowModel model)
    {
        return new SagePresenter(
            model,
            World.WindowView,
            activity => World.GameDataLoaded ? TestComposition.Assistant(World, activity) : null,
            () => World.PlayerName,
            () => World.AssistantName,
            World.Log,
            () => World.ShowThinking,
            () => World.ShowToolCalls,
            () => World.ShowToolResults,
            () => World.Conversation,
            clock: () =>
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
        return World.Ai == null
            ? Array.Empty<string>()
            : World.Ai.Transcript
                .SelectMany(messages => messages)
                .Where(message => message.Role == ChatRole.User)
                .Select(message => message.Content)
                .ToArray();
    }

    /// <summary>
    /// The true console parsing (<see cref="AiConsoleRouter"/>, the same
    /// object the game's <c>ai</c> command hands off to), wired to the
    /// scenario presenter. The shell splits the raw line on spaces, so
    /// the step does the same after stripping the <c>ai</c> prefix.
    /// </summary>
    private AiConsoleRouter ConsoleRouter()
    {
        return new AiConsoleRouter(() => World.WindowPresenter);
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
        World.WindowPresenter = BuildPresenter(World.Window);
    }

    [Given("the assistant is still answering {string}")]
    public async Task GivenTheAssistantIsStillAnswering(string question)
    {
        EnsurePresenter().ShouldNotBeNull();
        World.Ai ??= new FakeAiClient();
        World.Ai.Hold = true;
        World.WindowView.Input = question;
        World.WindowView.RaiseSubmit();
        World.WindowPresenter.Busy.ShouldBeTrue();
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
        World.Ai ??= new FakeAiClient();
        World.Ai.PauseAfterCalls = count;
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
        World.AskTask = World.WindowPresenter.AskAsync(question);
    }

    [When("the user types the command {string}")]
    public void WhenTheUserTypesTheCommand(string command)
    {
        EnsurePresenter().ShouldNotBeNull();
        _clockArmed = true;
        World.ConsoleOutput = ConsoleRouter().Execute(SplitConsoleLine(command));
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

    [When("the turn completes")]
    public async Task WhenTheTurnCompletes()
    {
        await WaitUntil(() => !World.WindowPresenter.Busy);
    }

    [When("the answer finishes")]
    public async Task WhenTheAnswerFinishes()
    {
        World.Ai.ShouldNotBeNull();
        World.Ai.Release();
        await WaitUntil(() => !World.WindowPresenter.Busy);
    }

    [When("the user clears the context")]
    public void WhenTheUserClearsTheContext()
    {
        EnsurePresenter().ShouldNotBeNull();
        World.WindowPresenter.ClearConversation();
    }

    [When("the user compacts the context")]
    public async Task WhenTheUserCompactsTheContext()
    {
        EnsurePresenter().ShouldNotBeNull();
        await World.WindowPresenter.CompactConversation();
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
