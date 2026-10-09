using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using Caev;
using StationeersAi.Test.Fakes;
using StationeersAi.Test.Support;
using Caev.Tools;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class SageAssistantSteps
{
    private readonly ScenarioContext _context;
    private int _nextCallId;

    public SageAssistantSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("the AI model will request the tool {string} with query {string}")]
    public void GivenTheAiModelWillRequestTheToolWithQuery(string tool, string query)
    {
        Model().RespondWith(new AiCompletion(null, new[]
        {
            new ToolCall($"call_{++_nextCallId}", tool, Arguments("query", query)),
        }));
    }

    [Given("the AI model will answer by repeating the last tool result")]
    public void GivenTheAiModelWillAnswerByRepeatingTheLastToolResult()
    {
        Model().EchoLastToolResult = true;
    }

    [Given("the AI model will answer {string}")]
    public void GivenTheAiModelWillAnswer(string answer)
    {
        Model().RespondWith(new AiCompletion(answer, Array.Empty<ToolCall>()));
    }

    [Given("the AI model reasons {string} and answers {string}")]
    public void GivenTheAiModelReasonsAndAnswers(string reasoning, string answer)
    {
        Model().RespondWith(new AiCompletion(answer, Array.Empty<ToolCall>(), reasoning));
    }

    [When("I ask the assistant {string}")]
    public async Task WhenIAskTheAssistant(string prompt)
    {
        await TestComposition.Gateway(World, World.Events).AskAsync(prompt, null);
    }

    [Then("the request sent back to the AI model should contain these messages:")]
    public void ThenTheRequestSentBackToTheAiModelShouldContain(Table table)
    {
        // The model's final request: it carries every tool result accumulated
        // so far, so it shows the whole conversation.
        World.Ai.Transcript.Count.ShouldBeGreaterThanOrEqualTo(1);
        var messages = World.Ai.Transcript[World.Ai.Transcript.Count - 1];

        messages.Count.ShouldBe(table.Rows.Count);

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var expected = table.Rows[i];
            var actual = messages[i];

            actual.Role.ToString().ToLowerInvariant()
                .ShouldBe(expected["Role"].ToLowerInvariant());

            (actual.Content ?? string.Empty)
                .ShouldBe((expected["Content"] ?? string.Empty).Replace("\\n", "\n"));

            var expectedTool = expected["Tool call"];
            if (string.IsNullOrEmpty(expectedTool))
            {
                (actual.ToolCalls == null || actual.ToolCalls.Count == 0).ShouldBeTrue(
                    "expected no tool call on this message");
            }
            else
            {
                actual.ToolCalls.ShouldNotBeNull();
                actual.ToolCalls.Single().Name.ShouldBe(expectedTool);
            }
        }
    }

    [Then("the assistant replies {string}")]
    public void ThenTheAssistantReplies(string expected)
    {
        World.Reply.ShouldBe(expected.Replace("\\n", "\n"));
    }

    private FakeAiClient Model()
    {
        if (World.Ai == null)
        {
            World.Ai = new FakeAiClient();
        }

        return World.Ai;
    }

    private static ToolArguments Arguments(string name, string value)
    {
        return new ToolArguments(new Dictionary<string, string> { [name] = value });
    }
}
