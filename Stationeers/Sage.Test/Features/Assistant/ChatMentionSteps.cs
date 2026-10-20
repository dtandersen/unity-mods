using System.Threading.Tasks;
using Reqnroll;
using Sage.Ai;
using Sage.Presentation;
using Sage.Test.Support;

namespace Sage.Test.Features.Assistant;

[Binding]
public sealed class ChatMentionSteps
{
    private readonly ScenarioContext _context;

    public ChatMentionSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    /// <summary>
    /// The scenario's one presenter: a second question must be refused by the same
    /// busy flag that gates the window, so specs never build their own.
    /// </summary>
    private SagePresenter Presenter => World.Presenter ??= TestComposition.Presenter(World);

    [When("the player mentions {string}")]
    public async Task WhenThePlayerMentions(string message)
    {
        await Presenter.OnChatLine(new ChatLine(World.PlayerName, message), World.ChatRules);
    }

    [When("I start a mention {string}")]
    public void WhenIStartAMention(string message)
    {
        World.AskTask = Presenter.OnChatLine(new ChatLine(World.PlayerName, message), World.ChatRules);
    }
}
