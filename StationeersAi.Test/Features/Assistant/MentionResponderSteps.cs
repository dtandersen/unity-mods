using System.Threading.Tasks;
using Reqnroll;
using StationeersAi.Ai;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class MentionResponderSteps
{
    private readonly ScenarioContext _context;

    public MentionResponderSteps(ScenarioContext context)
    {
        _context = context;
    }

    private World World => Hooks.GetWorld(_context);

    [When("the player mentions {string}")]
    public async Task WhenThePlayerMentions(string message)
    {
        World.MentionResponder ??= TestComposition.Responder(World);
        Mention.TryParse(World.MentionTrigger, message, out var prompt);
        await World.MentionResponder.HandleAsync(prompt, World.Conversation);
    }

    [When("I start a mention {string}")]
    public void WhenIStartAMention(string message)
    {
        World.MentionResponder ??= TestComposition.Responder(World);
        Mention.TryParse(World.MentionTrigger, message, out var prompt);
        World.AskTask = World.MentionResponder.HandleAsync(prompt, World.Conversation);
    }
}
