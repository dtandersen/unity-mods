using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OdinsAi.Ai;
using OdinsAi.Ai.OpenAi;
using OdinsAi.Test.Fakes;
using OdinsAi.Test.Support;
using OdinsAi.Tool;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Ai
{
    [Binding]
    public sealed class OpenAiWireSteps
    {
        private readonly ScenarioContext _context;

        public OpenAiWireSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("a local AI server")]
        public void GivenALocalAiServer()
        {
            World.Server = new FakeOpenAiServer();
        }

        [Given("the server will reply {int} with:")]
        public void GivenTheServerWillReplyWith(int status, string body)
        {
            World.Server.ShouldNotBeNull();
            World.Server.RespondWith(status, body);
        }

        [When("I complete a chat with {string} using model {string}")]
        public Task WhenICompleteAChatWith(string user, string model)
        {
            return CompleteAsync(user, model, Array.Empty<ToolDefinition>());
        }

        [When("I complete a chat with {string} with the {string} tool")]
        public Task WhenICompleteAChatWithTool(string user, string tool)
        {
            var definitions = TestComposition.Tools(World).Definitions
                .Where(definition => definition.Name == tool)
                .ToArray();

            return CompleteAsync(user, "test-model", definitions);
        }

        [Then("the completion content is {string}")]
        public void ThenTheCompletionContentIs(string expected)
        {
            World.Completion.ShouldNotBeNull();
            World.Completion.Content.ShouldBe(expected);
        }

        [Then("the completion requests the tool {string}")]
        public void ThenTheCompletionRequestsTheTool(string name)
        {
            World.Completion.ShouldNotBeNull();
            World.Completion.ToolCalls.ShouldHaveSingleItem().Name.ShouldBe(name);
        }

        [Then("the tool arguments contain {string} = {string}")]
        public void ThenTheToolArgumentsContain(string name, string value)
        {
            World.Completion.ToolCalls.Single().Arguments.Get(name).ShouldBe(value);
        }

        [Then("the server received a request to {string}")]
        public void ThenTheServerReceivedARequestTo(string path)
        {
            World.Server.Requests.ShouldNotBeEmpty();
            World.Server.Requests.Last().Path.ShouldBe(path);
        }

        [Then("the request body contains {string}")]
        public void ThenTheRequestBodyContains(string text)
        {
            World.Server.Requests.Last().Body.ShouldContain(text);
        }

        [Then("the completion fails with {string}")]
        public void ThenTheCompletionFailsWith(string text)
        {
            World.CompletionError.ShouldNotBeNull();
            World.CompletionError.Message.ShouldContain(text);
        }

        private async Task CompleteAsync(string user, string model, IReadOnlyList<ToolDefinition> tools)
        {
            var client = new OpenAiChatClient(
                new HttpChatTransport(),
                new OpenAiOptions(World.Server.ChatCompletionsUrl, "test-key", model));

            try
            {
                World.Completion = await client.CompleteAsync(
                    new[] { new ChatMessage(ChatRole.User, user) }, tools);
                World.CompletionError = null;
            }
            catch (AiClientException ex)
            {
                World.Completion = null;
                World.CompletionError = ex;
            }
        }
    }
}