using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Caev;
using Caev.OpenAi;
using Caev.Test.Fakes;
using Caev.Tools;
using Reqnroll;
using Shouldly;

namespace Caev.Test.Features.Ai;

[Binding]
public sealed class OpenAiWireSteps
{
    private FakeOpenAiServer _server;
    private AiCompletion _completion;
    private AiClientException _error;

    [Given("a local AI server")]
    public void GivenALocalAiServer()
    {
        _server ??= new FakeOpenAiServer();
    }

    [Given("the server will reply {int} with:")]
    public void GivenTheServerWillReplyWith(int status, string body)
    {
        _server.ShouldNotBeNull();
        _server.RespondWith(status, body);
    }

    [When("I complete a chat with {string} using model {string}")]
    public Task WhenICompleteAChatWith(string user, string model)
    {
        return CompleteAsync(user, model, new ToolDefinition[0], null);
    }

    [When("I complete a chat with {string} with the {string} tool")]
    public Task WhenICompleteAChatWithTool(string user, string tool)
    {
        var definitions = new[]
        {
            new ToolDefinition(tool, "Looks things up.", new[]
            {
                new ToolParameter("query", "What to look up."),
            }),
        };

        return CompleteAsync(user, "test-model", definitions, null);
    }

    [When("I complete a chat with {string} using reasoning effort {string}")]
    public Task WhenICompleteAChatWithReasoningEffort(string user, string effort)
    {
        return CompleteAsync(user, "test-model", new ToolDefinition[0], effort);
    }

    [Then("the completion content is {string}")]
    public void ThenTheCompletionContentIs(string expected)
    {
        _completion.ShouldNotBeNull();
        _completion.Content.ShouldBe(expected);
    }

    [Then("the completion reasoning is {string}")]
    public void ThenTheCompletionReasoningIs(string expected)
    {
        _completion.ShouldNotBeNull();
        _completion.Reasoning.ShouldBe(expected);
    }

    [Then("the completion requests the tool {string}")]
    public void ThenTheCompletionRequestsTheTool(string name)
    {
        _completion.ShouldNotBeNull();
        _completion.ToolCalls.ShouldHaveSingleItem().Name.ShouldBe(name);
    }

    [Then("the tool arguments contain {string} = {string}")]
    public void ThenTheToolArgumentsContain(string name, string value)
    {
        _completion.ToolCalls.Single().Arguments.Get(name).ShouldBe(value);
    }

    [Then("the server received a request to {string}")]
    public void ThenTheServerReceivedARequestTo(string path)
    {
        _server.Requests.ShouldNotBeEmpty();
        _server.Requests.Last().Path.ShouldBe(path);
    }

    [Then("the request body contains {string}")]
    public void ThenTheRequestBodyContains(string text)
    {
        _server.Requests.Last().Body.ShouldContain(text);
    }

    [Then("the completion fails with {string}")]
    public void ThenTheCompletionFailsWith(string text)
    {
        _error.ShouldNotBeNull();
        _error.Message.ShouldContain(text);
    }

    private async Task CompleteAsync(string user, string model, IReadOnlyList<ToolDefinition> tools, string reasoningEffort)
    {
        var client = new OpenAiChatClient(
            new HttpChatTransport(new HttpClient()),
            new OpenAiOptions(_server.ChatCompletionsUrl, "test-key", model, reasoningEffort));

        try
        {
            _completion = await client.CompleteAsync(
                new[] { new ChatMessage(ChatRole.User, user) }, tools);
            _error = null;
        }
        catch (AiClientException ex)
        {
            _completion = null;
            _error = ex;
        }
    }
}
