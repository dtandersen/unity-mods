using System.Threading.Tasks;
using OdinsAi.Ai;
using OdinsAi.Test.Support;
using Reqnroll;

namespace OdinsAi.Test.Features.Assistant
{
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
            await World.MentionResponder.HandleAsync(prompt);
        }

        [When("I start a mention {string}")]
        public void WhenIStartAMention(string message)
        {
            World.MentionResponder ??= TestComposition.Responder(World);
            Mention.TryParse(World.MentionTrigger, message, out var prompt);
            World.AskTask = World.MentionResponder.HandleAsync(prompt);
        }
    }
}