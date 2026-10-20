using Caev;
using Sage.Core;
using Sage.Command;
using Sage.Query;
using Caev.Tools;
using System;
using Sage.Ai;
using Sage.Test.Fakes;
using Sage.Presentation;
using Sage.Tool;

namespace Sage.Test.Support;

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
            new SearchStructuresTool(new SearchStructuresCommand(world.Structures)),
            new DescribeStructureTool(new DescribeStructureCommand(world.StructureInspector, world.Structures)),
            new DescribeThingTool(new DescribeThingCommand(world.ThingInspector, world.Structures, world.Things)),
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
