using OdinsAi.Ai;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Assistant
{
    [Binding]
    public sealed class MentionSteps
    {
        private bool _recognised;
        private string _prompt;

        [When("the mention trigger is {string} and the message is {string}")]
        public void WhenTheMentionTriggerIsAndTheMessageIs(string trigger, string message)
        {
            _recognised = Mention.TryParse(trigger, message, out _prompt);
        }

        [Then("the mention is recognised as {string}")]
        public void ThenTheMentionIsRecognisedAs(string expected)
        {
            _recognised.ShouldBeTrue();
            _prompt.ShouldBe(expected);
        }

        [Then("the message is not a mention")]
        public void ThenTheMessageIsNotAMention()
        {
            _recognised.ShouldBeFalse();
        }
    }
}