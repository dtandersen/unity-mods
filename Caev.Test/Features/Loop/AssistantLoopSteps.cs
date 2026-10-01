using System.Collections.Generic;
using System.Threading.Tasks;
using Caev;
using Caev.Test.Fakes;
using Caev.Tools;
using Reqnroll;
using Shouldly;

namespace Caev.Test.Features.Loop;

[Binding]
public sealed class AssistantLoopSteps
{
    private readonly FakeAiClient _ai = new FakeAiClient();
    private readonly StubTool _lookup = new StubTool("lookup");
    private readonly RecordingActivity _activity = new RecordingActivity();
    private readonly RecordingLogger _log = new RecordingLogger();
    private readonly Conversation _conversation = new Conversation();
    private int _nextCallId;
    private string _reply;

    private AiAssistant Assistant(int maxIterations)
    {
        return new AiAssistant(
            _ai,
            new ToolHandler(new ITool[] { _lookup }),
            systemPrompt: null,
            maxIterations: maxIterations,
            log: _log,
            activity: _activity);
    }

    [Given("the AI model will answer {string}")]
    public void GivenTheAiModelWillAnswer(string answer)
    {
        _ai.RespondWith(new AiCompletion(answer, new ToolCall[0]));
    }

    [Given("the AI model will request the tool {string} with query {string}")]
    public void GivenTheAiModelWillRequestTheTool(string tool, string query)
    {
        _ai.RespondWith(new AiCompletion(null, new[]
        {
            new ToolCall($"call_{++_nextCallId}", tool, new ToolArguments(
                new Dictionary<string, string> { ["query"] = query })),
        }));
    }

    [Given("we discussed {string} and heard {string}")]
    public void GivenWeDiscussedAndHeard(string question, string answer)
    {
        _conversation.Add(new ChatMessage(ChatRole.User, question));
        _conversation.Add(new ChatMessage(ChatRole.Assistant, answer));
    }

    [When("the assistant is asked {string}")]
    public async Task WhenTheAssistantIsAsked(string prompt)
    {
        _reply = await Assistant(5).AskAsync(prompt, null);
    }

    [When("the assistant is asked {string} with {int} iterations")]
    public async Task WhenTheAssistantIsAskedWithIterations(string prompt, int iterations)
    {
        _reply = await Assistant(iterations).AskAsync(prompt, null);
    }

    [When("the conversation is compacted")]
    public async Task WhenTheConversationIsCompacted()
    {
        await Assistant(5).CompactAsync(_conversation);
    }

    [Then("the assistant replies {string}")]
    public void ThenTheAssistantReplies(string expected)
    {
        _reply.ShouldBe(expected);
    }

    [Then("the tool {string} ran with query {string}")]
    public void ThenTheToolRanWithQuery(string name, string query)
    {
        name.ShouldBe(_lookup.Name);
        _lookup.Calls.Count.ShouldBe(1);
        _lookup.Calls[0].Get("query").ShouldBe(query);
    }

    [Then("the assistant reports it could not complete")]
    public void ThenTheAssistantReportsItCouldNotComplete()
    {
        _reply.ShouldContain("couldn't complete");
    }

    [Then("the conversation holds the summary {string}")]
    public void ThenTheConversationHoldsTheSummary(string expected)
    {
        _conversation.Messages.Count.ShouldBe(1);
        _conversation.Messages[0].Role.ShouldBe(ChatRole.Assistant);
        _conversation.Messages[0].Content.ShouldBe(expected);
    }
}
