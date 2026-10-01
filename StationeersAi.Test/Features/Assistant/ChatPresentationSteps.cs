using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class ChatPresentationSteps
{
    private readonly ScenarioContext _context;

    public ChatPresentationSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [Given("game data is not loaded")]
    public void GivenGameDataIsNotLoaded()
    {
        World.GameDataLoaded = false;
    }

    [Given("my player name is {string}")]
    public void GivenMyPlayerNameIs(string name)
    {
        World.PlayerName = name;
    }

    [Given("the assistant is named {string}")]
    public void GivenTheAssistantIsNamed(string name)
    {
        World.AssistantName = name;
    }

    [Given("thinking display is off")]
    public void GivenThinkingDisplayIsOff()
    {
        World.ShowThinking = false;
    }

    [Given("thinking display is on")]
    public void GivenThinkingDisplayIsOn()
    {
        World.ShowThinking = true;
    }

    [Given("the AI model will wait")]
    public void GivenTheAiModelWillWait()
    {
        World.Ai ??= new FakeAiClient();
        World.Ai.Hold = true;
    }

    [When("I ask the assistant in chat {string}")]
    public async Task WhenIAskTheAssistantInChat(string prompt)
    {
        World.Ai ??= new FakeAiClient();
        await TestComposition.Presenter(World).AskAsync(prompt, null);
    }

    [When("I release the AI model")]
    public async Task WhenIReleaseTheAiModel()
    {
        World.Ai.Release();
        await World.AskTask;
    }

    [Then("the chat shows:")]
    public void ThenTheChatShows(string expected)
    {
        var lines = expected.Replace("\r\n", "\n").Trim()
            .Split('\n')
            .Select(line => line.Trim())
            .ToArray();

        World.Chat.Lines.ShouldBe(lines);
    }
}
