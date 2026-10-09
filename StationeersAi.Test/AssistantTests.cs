using System;
using System.Threading.Tasks;
using Shouldly;
using Caev;
using StationeersAi.Core;
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

    [Test]
    public async Task Mention_responder_answers_once_at_a_time()
    {
        var chat = new RecordingChatSink();
        var ai = new FakeAiClient().RespondWith(Answer("hello"));
        var responder = new MentionResponder(
            chat,
            events => new CaevAgentGateway(ai, RecipeTools(), new RecordingLogger(), events),
            new FakeAgentSettings(),
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

}
