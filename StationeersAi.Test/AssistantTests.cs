using System;
using System.Threading.Tasks;
using Shouldly;
using Caev;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Test.Fakes;
using Caev.Tools;
using StationeersAi.Ai;
using StationeersAi.Tool;

namespace StationeersAi.Test;

/// <summary>Chat presentation and mention answering.</summary>
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
    public async Task Presenter_echoes_question_and_posts_plain_answer()
    {
        var chat = new RecordingChatSink();
        var ai = new FakeAiClient().RespondWith(Answer("**Cable** Coil needs `5` copper."));
        var presenter = new ChatPresenter(
            chat,
            _ => new AiAssistant(ai, RecipeTools(), systemPrompt: null, maxIterations: 5, log: new RecordingLogger(), activity: null),
            () => "Tester",
            "Sage",
            new RecordingLogger(),
            showThinking: false,
            showToolCalls: false,
            showToolResults: false);

        await presenter.AskAsync("how do I make cable coil?", null);

        chat.Lines.Count.ShouldBe(2);
        chat.Lines[0].ShouldBe("Tester: how do I make cable coil?");
        chat.Lines[1].ShouldBe("Sage: Cable Coil needs 5 copper.");
    }

    [Test]
    public async Task Presenter_reports_game_data_not_loaded()
    {
        var chat = new RecordingChatSink();
        var presenter = new ChatPresenter(chat, _ => null, () => "Tester", "Sage", new RecordingLogger(), showThinking: false, showToolCalls: false, showToolResults: false);

        await presenter.AskAsync("hi", null);

        chat.Lines[^1].ShouldContain("game data is not loaded yet.");
    }

    [Test]
    public async Task Mention_responder_answers_once_at_a_time()
    {
        var chat = new RecordingChatSink();
        var ai = new FakeAiClient().RespondWith(Answer("hello"));
        var responder = new MentionResponder(
            chat,
            activity => new AiAssistant(ai, RecipeTools(), systemPrompt: null, maxIterations: 5, log: new RecordingLogger(), activity: activity),
            "Sage",
            new RecordingLogger());

        (await responder.HandleAsync("hi", null)).ShouldBeTrue();
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
    public void Conversation_drops_oldest_turn_whole()
    {
        var conversation = new Conversation(2);
        conversation.Add(new ChatMessage(ChatRole.User, "one"));
        conversation.Add(new ChatMessage(ChatRole.Assistant, "two"));
        conversation.Add(new ChatMessage(ChatRole.User, "three"));

        conversation.Messages.Count.ShouldBe(1);
        conversation.Messages[0].Content.ShouldBe("three");
    }
}
