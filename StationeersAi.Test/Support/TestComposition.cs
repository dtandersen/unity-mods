using Caev;
using StationeersAi.Command;
using StationeersAi.Query;
using Caev.Tools;
using StationeersAi.Ai;
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
            new GetStatusTool(new GetPlayerCommand(world.Player)),
            new GetWorldTool(new GetWorldCommand(world.Worlds)),
            new WriteIcTool(new WriteIcCommand(world.Ic)),
            new ReadIcTool(new ReadIcCommand(world.Ic)),
            new ExportIcTool(new ExportIcCommand(world.Export)),
            new MipsReferenceTool(new ReadInstructionsCommand(world.Mips)),
            new SearchDevicesTool(new SearchDevicesCommand(world.Devices)),
            new DescribeDeviceTool(new DescribeDeviceCommand(world.Inspector, world.Devices)),
            new SearchThingsTool(new SearchThingsCommand(world.Things, world.Player)),
            new ReadRoomTool(new ReadRoomCommand(world.RoomInspector, world.Player)),
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
            activity => Assistant(world, activity),
            world.AssistantName,
            world.Log);
    }
}
