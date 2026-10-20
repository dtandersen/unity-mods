using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using Sage.Test.Fakes;
using Sage.Test.Support;

namespace Sage.Test.Features.Assistant;

[Binding]
public sealed class ChatPresentationSteps
{
    private readonly ScenarioContext _context;

    public ChatPresentationSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

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

    [Given("thinking display is on")]
    public void GivenThinkingDisplayIsOn()
    {
        World.ShowThinking = true;
    }

    [Given("the AI model will wait")]
    public void GivenTheAiModelWillWait()
    {
        World.Ai.Hold = true;
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
