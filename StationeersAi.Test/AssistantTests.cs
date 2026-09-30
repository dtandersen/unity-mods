using System;
using System.Threading.Tasks;
using Shouldly;
using StationeersAi.Ai;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Test.Fakes;
using StationeersAi.Tool;

namespace StationeersAi.Test
{
    /// <summary>The model/tool loop, conversation memory, and chat presentation.</summary>
    public class AssistantTests
    {
        private static ToolHandler RecipeTools()
        {
            var recipes = new InMemoryRecipeRepository(new[]
            {
                new Recipe("ItemCableCoil", "ItemCableCoil", "Cable Coil", 1, "Electronics Printer",
                    new[] { new Ingredient("Copper", 5) }),
            });

            return new ToolHandler(new ITool[]
            {
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(recipes))),
            });
        }

        private static AiCompletion Answer(string text) =>
            new AiCompletion(text, Array.Empty<ToolCall>());

        private static AiCompletion CallTool(string id, string name, string query) =>
            new AiCompletion(null, new[]
            {
                new ToolCall(id, name, new ToolArguments(
                    new System.Collections.Generic.Dictionary<string, string> { ["query"] = query })),
            });

        [Test]
        public async Task Ask_runs_tool_loop_then_answers()
        {
            var ai = new FakeAiClient()
                .RespondWith(CallTool("1", "search_recipes", "cable"))
                .RespondWith(Answer("Cable Coil needs 5 copper."));
            var assistant = new AiAssistant(ai, RecipeTools(), log: new RecordingLogger());

            var answer = await assistant.AskAsync("how do I make cable coil?");

            answer.ShouldBe("Cable Coil needs 5 copper.");
            ai.Transcript.Count.ShouldBe(2);
            // The tool result was fed back to the model.
            ai.Transcript[1][^1].Role.ShouldBe(ChatRole.Tool);
            ai.Transcript[1][^1].Content.ShouldContain("Cable Coil");
        }

        [Test]
        public async Task Ask_appends_turn_to_conversation()
        {
            var ai = new FakeAiClient().RespondWith(Answer("hi"));
            var assistant = new AiAssistant(ai, RecipeTools(), log: new RecordingLogger());
            var conversation = new Conversation();

            await assistant.AskAsync("hello", conversation);

            conversation.Messages.Count.ShouldBe(2);
            conversation.Messages[0].Role.ShouldBe(ChatRole.User);
            conversation.Messages[1].Role.ShouldBe(ChatRole.Assistant);
        }

        [Test]
        public async Task Ask_gives_up_after_max_iterations()
        {
            var ai = new FakeAiClient { EchoLastToolResult = false }
                .RespondWith(CallTool("1", "search_recipes", "cable"))
                .RespondWith(CallTool("2", "search_recipes", "cable"));
            var assistant = new AiAssistant(ai, RecipeTools(), log: new RecordingLogger(), maxIterations: 2);

            var answer = await assistant.AskAsync("cable?");

            answer.ShouldContain("couldn't complete");
        }

        [Test]
        public async Task Compact_replaces_history_with_summary()
        {
            var ai = new FakeAiClient().RespondWith(Answer("user asked about cable"));
            var assistant = new AiAssistant(ai, RecipeTools(), log: new RecordingLogger());
            var conversation = new Conversation();
            conversation.Add(new ChatMessage(ChatRole.User, "cable?"));
            conversation.Add(new ChatMessage(ChatRole.Assistant, "5 copper"));

            var summary = await assistant.CompactAsync(conversation);

            summary.ShouldBe("user asked about cable");
            conversation.Messages.Count.ShouldBe(1);
        }

        [Test]
        public async Task Presenter_echoes_question_and_posts_plain_answer()
        {
            var chat = new RecordingChatSink();
            var ai = new FakeAiClient().RespondWith(Answer("**Cable** Coil needs `5` copper."));
            var presenter = new ChatPresenter(
                chat,
                _ => new AiAssistant(ai, RecipeTools(), log: new RecordingLogger()),
                () => "Tester",
                "Sage",
                new RecordingLogger());

            await presenter.AskAsync("how do I make cable coil?");

            chat.Lines.Count.ShouldBe(2);
            chat.Lines[0].ShouldBe("Tester: how do I make cable coil?");
            chat.Lines[1].ShouldBe("Sage: Cable Coil needs 5 copper.");
        }

        [Test]
        public async Task Presenter_reports_game_data_not_loaded()
        {
            var chat = new RecordingChatSink();
            var presenter = new ChatPresenter(chat, _ => null, () => "Tester", "Sage", new RecordingLogger());

            await presenter.AskAsync("hi");

            chat.Lines[^1].ShouldContain("game data is not loaded yet.");
        }

        [Test]
        public async Task Mention_responder_answers_once_at_a_time()
        {
            var chat = new RecordingChatSink();
            var ai = new FakeAiClient().RespondWith(Answer("hello"));
            var responder = new MentionResponder(
                chat,
                () => new AiAssistant(ai, RecipeTools(), log: new RecordingLogger()),
                "Sage",
                new RecordingLogger());

            (await responder.HandleAsync("hi")).ShouldBeTrue();
            responder.IsBusy.ShouldBeFalse();
            chat.Lines.Count.ShouldBe(1);
            chat.Lines[0].ShouldBe("Sage: hello");
        }

        [Test]
        public void Mention_parses_trigger_with_boundary()
        {
            Mention.TryParse("!sage", "!sage how do I make cable?", out var prompt).ShouldBeTrue();
            prompt.ShouldBe("how do I make cable?");
            Mention.TryParse("!sage", "!sageson", out _).ShouldBeFalse();
            Mention.TryParse("!sage", "!sage", out _).ShouldBeFalse();
        }

        [Test]
        public void Conversation_drops_oldest_messages()
        {
            var conversation = new Conversation(2);
            conversation.Add(new ChatMessage(ChatRole.User, "one"));
            conversation.Add(new ChatMessage(ChatRole.Assistant, "two"));
            conversation.Add(new ChatMessage(ChatRole.User, "three"));

            conversation.Messages.Count.ShouldBe(2);
            conversation.Messages[0].Content.ShouldBe("two");
        }
    }
}
