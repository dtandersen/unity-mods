using Caev;
using StationeersAi.Core;
using StationeersAi.Command;
using StationeersAi.Query;
using Caev.Tools;
using System;
using StationeersAi.Ai;
using StationeersAi.Test.Fakes;
using StationeersAi.Presentation;
using StationeersAi.Tool;

namespace StationeersAi.Test.Support;

/// <summary>
/// Wires the real core objects for a scenario. Tests exercise public entry
/// points (<see cref="ToolHandler"/>, <see cref="CaevAgentGateway"/>) rather than
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
            new ReadPipeNetworkTool(new ReadPipeNetworkCommand(world.PipeNetworks)),
        });
    }

    public static AgentGateway Gateway(WorldContext world)
    {
        return new CaevAgentGateway(world.Ai, Tools(world), world.Log, world.Events);
    }

    /// <summary>
    /// The one presenter for a scenario: the only object that starts a turn, asked
    /// through the scenario gateway, answering to the scenario chat view.
    /// </summary>
    public static SagePresenter Presenter(WorldContext world, Func<DateTimeOffset> clock = null)
    {
        world.WindowView ??= new FakeSageView();
        return new SagePresenter(
            world.Window,
            world.WindowView,
            world.Chat,
            world.Gateway,
            world,
            () => world.PlayerName,
            () => world.AssistantName,
            world.Log,
            () => world.ShowThinking,
            () => world.ShowToolCalls,
            () => world.ShowToolResults,
            () => world.Conversation,
            clock: clock);
    }
}
