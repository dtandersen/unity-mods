using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using Caev;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;
using Caev.Tools;
using Caev.OpenAi;

namespace StationeersAi.Test.Features.Ai;

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
        return CompleteAsync(user, model, Array.Empty<ToolDefinition>(), null);
    }

    [When("I complete a chat with {string} with the {string} tool")]
    public Task WhenICompleteAChatWithTool(string user, string tool)
    {
        var definitions = TestComposition.Tools(World).Definitions
            .Where(definition => definition.Name == tool)
            .ToArray();

        return CompleteAsync(user, "test-model", definitions, null);
    }

    [When("I complete a chat with {string} using reasoning effort {string}")]
    public Task WhenICompleteAChatWithReasoningEffort(string user, string effort)
    {
        return CompleteAsync(user, "test-model", Array.Empty<ToolDefinition>(), effort);
    }

    [Then("the completion content is {string}")]
    public void ThenTheCompletionContentIs(string expected)
    {
        World.Completion.ShouldNotBeNull();
        World.Completion.Content.ShouldBe(expected);
    }

    [Then("the completion reasoning is {string}")]
    public void ThenTheCompletionReasoningIs(string expected)
    {
        World.Completion.ShouldNotBeNull();
        World.Completion.Reasoning.ShouldBe(expected);
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

    private async Task CompleteAsync(string user, string model, IReadOnlyList<ToolDefinition> tools, string reasoningEffort)
    {
        var client = new OpenAiChatClient(
            new HttpChatTransport(new HttpClient()),
            new OpenAiOptions(World.Server.ChatCompletionsUrl, "test-key", model, reasoningEffort));

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
