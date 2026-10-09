using System.Collections.Generic;
using System.Linq;
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
    private readonly AgentEvents _events = new AgentEvents();
    private readonly EventRecorder _reported;
    private readonly RecordingLogger _log = new RecordingLogger();
    private readonly Conversation _conversation = new Conversation();
    private int _nextCallId;
    private string _systemPrompt;
    private int _turnIterations = 5;

    /// <summary>The answer the last turn reported.</summary>
    private string Answer => _reported.Answers.LastOrDefault() ?? string.Empty;

    public AssistantLoopSteps()
    {
        _reported = new EventRecorder(_events);
    }

    private Agent BuildAgent()
    {
        return new Agent(_ai, new ToolHandler(new ITool[] { _lookup }), _log, _events);
    }

    /// <summary>A turn as the engine sees it: question, history, instructions, limit.</summary>
    private TurnContext Turn(string prompt, Conversation conversation = null, int maxIterations = -1)
        => new TurnContext(prompt, conversation, _systemPrompt, maxIterations < 0 ? _turnIterations : maxIterations);

    [Given("the model {string} is not a valid model")]
    public void GivenTheModelIsNotAValidModel(string model)
    {
        _ai.Model = model;
        _ai.FailWith(404);
    }

    [Given("the model {string} returns a {int}")]
    public void GivenTheModelReturnsAStatus(string model, int statusCode)
    {
        _ai.Model = model;
        _ai.FailWith(statusCode);
    }

    [When("we ask the question {string}")]
    public async Task WhenWeAskTheQuestion(string prompt)
    {
        await BuildAgent().AskAsync(Turn(prompt, _conversation));
    }

    [Then("the agent answers with:")]
    public void ThenTheAgentAnswersWith(string expected)
    {
        Flat(Answer).ShouldBe(Flat(expected));
    }

    /// <summary>Docstrings and answers may differ only in line breaks and padding.</summary>
    private static string Flat(string text) => text.Replace("\r\n", "\n").Trim();

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

    [Given("the AI model reasons {string} and answers {string}")]
    public void GivenTheAiModelReasonsAndAnswers(string reasoning, string answer)
    {
        _ai.RespondWith(new AiCompletion(answer, new ToolCall[0], reasoning));
    }

    [Given("the tool {string} will answer {string}")]
    public void GivenTheToolWillAnswer(string name, string answer)
    {
        name.ShouldBe(_lookup.Name);
        _lookup.AnswersWith(answer);
    }

    [Given("the turn allows {int} tool rounds")]
    public void GivenTheTurnAllowsToolRounds(int rounds)
    {
        _turnIterations = rounds;
    }

    [Given("the turn instructs {string}")]
    public void GivenTheTurnInstructs(string instructions)
    {
        _systemPrompt = instructions;
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
        await BuildAgent().AskAsync(Turn(prompt));
    }

    [When("the conversation is compacted")]
    public async Task WhenTheConversationIsCompacted()
    {
        await BuildAgent().CompactAsync(_conversation);
    }

    [Then("the instructions sent {string}")]
    public void ThenTheInstructionsSent(string expected)
    {
        var sent = _ai.Transcript[_ai.Transcript.Count - 1];
        sent[0].Role.ShouldBe(ChatRole.System);
        sent[0].Content.ShouldBe(expected);
    }

    [Then("the assistant replies {string}")]
    public void ThenTheAssistantReplies(string expected)
    {
        Answer.ShouldBe(expected);
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
        Answer.ShouldContain("couldn't complete");
    }

    [Then("the answer reported {string}")]
    public void ThenTheAnswerReported(string expected)
    {
        _reported.Answers.ShouldBe(new[] { expected });
    }

    [Then("the reasoning reported {string}")]
    public void ThenTheReasoningReported(string expected)
    {
        _reported.Reasoning.ShouldContain(expected);
    }

    [Then("the tool call reported {string}")]
    public void ThenTheToolCallReported(string expected)
    {
        _reported.ToolCalls.ShouldContain(expected);
    }

    [Then("the tool output reported {string}")]
    public void ThenTheToolOutputReported(string expected)
    {
        _reported.ToolOutputs.ShouldContain(expected);
    }

    [Then("the conversation holds the summary {string}")]
    public void ThenTheConversationHoldsTheSummary(string expected)
    {
        _conversation.Messages.Count.ShouldBe(1);
        _conversation.Messages[0].Role.ShouldBe(ChatRole.Assistant);
        _conversation.Messages[0].Content.ShouldBe(expected);
    }
}
