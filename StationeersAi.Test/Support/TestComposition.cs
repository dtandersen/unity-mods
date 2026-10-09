using Caev;
using StationeersAi.Command;
using StationeersAi.Query;
using Caev.Tools;
using StationeersAi.Ai;
using StationeersAi.Tool;

namespace StationeersAi.Test.Support;

/// <summary>
/// Wires the real core objects for a scenario. Tests exercise public entry
/// points (<see cref="ToolHandler"/>, <see cref="Agent"/>) rather than
/// constructing internals inline.
/// </summary>
internal static class TestComposition
{
    public static ToolHandler Tools(WorldContext world)
    {
        return new ToolHandler(new ITool[]
        {
            new SearchItemsTool(new SearchItemsCommand(new ItemIndex(world.Items))),
            new SearchStructuresTool(new SearchStructuresCommand(new StructureIndex(world.Structures))),
            new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(world.Recipes))),
            new SearchStationpediaTool(new SearchStationpediaCommand(new StationpediaIndex(world.Stationpedia))),
            new ReadStationpediaTool(new ReadStationpediaCommand(world.Stationpedia)),
            new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(world.Player))),
            new DescribePlayerTool(new DescribePlayerCommand(world.Player)),
            new GetWorldTool(new GetWorldCommand(world.Worlds)),
            new WriteIcTool(new WriteComputerCommand(world.IcDevices)),
            new ReadIcTool(new ReadComputerCommand(world.IcDevices), new ReadHousingCommand(world.IcDevices)),
            new ExportIcTool(new ExportChipCommand(world.IcDevices)),
            new ImportIcTool(new ImportChipCommand(world.IcDevices)),
            new SelectIcTool(new SelectHousingCommand(world.IcDevices)),
            new ListInstructionsTool(new ListInstructionsCommand(world.Mips)),
            new SearchDevicesTool(new SearchDevicesCommand(world.Devices)),
            new DescribeDeviceTool(new DescribeDeviceCommand(world.Inspector, world.Devices)),
            new SearchThingsTool(new SearchThingsCommand(world.Things, world.Player)),
            new ReadSlotsTool(new ReadThingSlotsCommand(world.ThingSlots)),
            new ReadRoomTool(new ReadRoomCommand(world.RoomInspector, world.Player)),
        });
    }

    public static AgentGateway Assistant(WorldContext world, AgentEvents events)
    {
        return new Agent(world.Ai, Tools(world), null, 5, world.Log, events);
    }

    public static MentionResponder Responder(WorldContext world)
    {
        return new MentionResponder(
            world.Chat,
            events => Assistant(world, events),
            world.AssistantName,
            world.Log);
    }
}
