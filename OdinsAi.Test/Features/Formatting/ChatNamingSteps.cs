using OdinsAi.Formatting;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Formatting
{
    [Binding]
    public sealed class ChatNamingSteps
    {
        private string _configured;

        [When("the assistant name is {string}")]
        public void WhenTheAssistantNameIs(string configured)
        {
            _configured = configured;
        }

        [Then("the assistant is called {string}")]
        public void ThenTheAssistantIsCalled(string expected)
        {
            ChatNaming.Resolve(_configured).ShouldBe(expected);
        }

        [Then("a chat line for {string} is {string}")]
        public void ThenAChatLineForIs(string text, string expected)
        {
            ChatNaming.Apply(_configured, text).ShouldBe(expected);
        }
    }
}