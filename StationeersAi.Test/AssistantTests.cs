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
    public void Mention_parses_trigger_with_boundary()
    {
        Mention.TryParse("!sage", "!sage how do I make cable?", out var prompt).ShouldBeTrue();
        prompt.ShouldBe("how do I make cable?");
        Mention.TryParse("!sage", "!sageson", out _).ShouldBeFalse();
        Mention.TryParse("!sage", "!sage", out _).ShouldBeFalse();
    }

}
