using StationeersAi.Ai;
using StationeersAi.Command;
using StationeersAi.Query;
using StationeersAi.Tool;

namespace StationeersAi.Test.Support;

/// <summary>
/// Wires the real core objects for a scenario. Tests exercise public entry
/// points (<see cref="ToolHandler"/>, <see cref="AiAssistant"/>) rather than
/// constructing internals inline.
/// </summary>
internal static class TestComposition
{
    public static ToolHandler Tools(World world)
    {
        return new ToolHandler(new ITool[]
        {
            new SearchItemsTool(new SearchItemsCommand(new ItemIndex(world.Items))),
            new SearchStructuresTool(new SearchStructuresCommand(new StructureIndex(world.Structures))),
            new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(world.Recipes))),
            new SearchStationpediaTool(new SearchStationpediaCommand(new StationpediaIndex(world.Stationpedia))),
            new ReadStationpediaTool(new ReadStationpediaCommand(world.Stationpedia)),
            new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(world.Player))),
            new GetStatusTool(new GetStatusCommand(world.Player)),
            new GetWorldTool(new GetWorldCommand(world.Worlds)),
            new WriteIcTool(new WriteIcCommand(world.Ic)),
            new ReadIcTool(new ReadIcCommand(world.Ic)),
            new ExportIcTool(new ExportIcCommand(world.Export)),
            new MipsReferenceTool(new ReadMipsCommand(world.Mips)),
            new SearchDevicesTool(new SearchDevicesCommand(world.Devices)),
            new DescribeDeviceTool(new DescribeDeviceCommand(world.Inspector, world.Devices)),
        });
    }

    public static AiAssistant Assistant(World world)
    {
        return new AiAssistant(world.Ai, Tools(world), systemPrompt: null, maxIterations: 5, log: world.Log, activity: null);
    }

    public static AiAssistant Assistant(World world, IAssistantActivity activity)
    {
        return new AiAssistant(world.Ai, Tools(world), null, 5, world.Log, activity);
    }

    public static MentionResponder Responder(World world)
    {
        return new MentionResponder(
            world.Chat,
            () => Assistant(world),
            world.AssistantName,
            world.Log);
    }

    public static ChatPresenter Presenter(World world)
    {
        return new ChatPresenter(
            world.Chat,
            activity => world.GameDataLoaded ? Assistant(world, activity) : null,
            () => world.PlayerName,
            world.AssistantName,
            world.Log,
            showThinking: world.ShowThinking,
            showToolCalls: world.ShowToolCalls,
            showToolResults: world.ShowToolResults,
            clock: world.Clock);
    }
}
