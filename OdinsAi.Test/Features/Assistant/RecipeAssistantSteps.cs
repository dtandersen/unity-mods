using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OdinsAi.Ai;
using OdinsAi.Test.Fakes;
using OdinsAi.Test.Support;
using OdinsAi.Tool;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Assistant
{
    [Binding]
    public sealed class RecipeAssistantSteps
    {
        private readonly ScenarioContext _context;

        public RecipeAssistantSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("the AI model will request the tool {string} with query {string}")]
        public void GivenTheAiModelWillRequestTheToolWithQuery(string tool, string query)
        {
            World.Ai = new FakeAiClient();
            World.Ai.RespondWith(new AiCompletion(null, new[]
            {
                new ToolCall("call_1", tool, Arguments("query", query)),
            }));
        }

        [Given("the AI model will answer by repeating the last tool result")]
        public void GivenTheAiModelWillAnswerByRepeatingTheLastToolResult()
        {
            World.Ai.ShouldNotBeNull();
            World.Ai.EchoLastToolResult = true;
        }

        [When("I ask the assistant {string}")]
        public async Task WhenIAskTheAssistant(string prompt)
        {
            World.Reply = await TestComposition.Assistant(World).AskAsync(prompt);
        }

        [Then("the request sent back to the AI model should contain these messages:")]
        public void ThenTheRequestSentBackToTheAiModelShouldContain(Table table)
        {
            // The model's second request carries the tool result. transcript[0] is
            // the first request (the user question alone).
            World.Ai.Transcript.Count.ShouldBeGreaterThanOrEqualTo(2);
            var messages = World.Ai.Transcript[1];

            messages.Count.ShouldBe(table.Rows.Count);

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var expected = table.Rows[i];
                var actual = messages[i];

                actual.Role.ToString().ToLowerInvariant()
                    .ShouldBe(expected["Role"].ToLowerInvariant());

                (actual.Content ?? string.Empty).ShouldBe(expected["Content"]);

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
            World.Reply.ShouldBe(expected);
        }

        private static ToolArguments Arguments(string name, string value)
        {
            return new ToolArguments(new Dictionary<string, string> { [name] = value });
        }
    }
}