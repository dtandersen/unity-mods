using Sage.Command;
using Sage.Query;
using Sage.Repository;

namespace Sage.Server;

/// <summary>
/// Builds a fresh command per call from the given readers. The game fills
/// this from live sources; tests fill it from in-memory fakes. The router
/// and the chat assistant read through the same object, so both serve the
/// same data. Holds no game state.
/// </summary>
public sealed class SageCommands
{
    private readonly IItemRepository _items;
    private readonly IRecipeRepository _recipes;
    private readonly IStationpediaRepository _stationpedia;
    private readonly IPlayerContext _player;
    private readonly IWorldContext _worlds;
    private readonly IIcDevices _icDevices;
    private readonly IMipsReference _mips;
    private readonly IStructureCatalog _structures;
    private readonly IStructureInspector _structureInspector;
    private readonly IThingInspector _thingInspector;
    private readonly IThingCatalog _things;
    private readonly IThingSlots _slots;
    private readonly IRoomInspector _rooms;
    private readonly IPipeNetworkInspector _pipeNetworks;

    public SageCommands(
        IItemRepository items,
        IRecipeRepository recipes,
        IStationpediaRepository stationpedia,
        IPlayerContext player,
        IWorldContext worlds,
        IIcDevices icDevices,
        IMipsReference mips,
        IStructureCatalog structures,
        IStructureInspector structureInspector,
        IThingInspector thingInspector,
        IThingCatalog things,
        IThingSlots slots,
        IRoomInspector rooms,
        IPipeNetworkInspector pipeNetworks)
    {
        _items = items;
        _recipes = recipes;
        _stationpedia = stationpedia;
        _player = player;
        _worlds = worlds;
        _icDevices = icDevices;
        _mips = mips;
        _structures = structures;
        _structureInspector = structureInspector;
        _thingInspector = thingInspector;
        _things = things;
        _slots = slots;
        _rooms = rooms;
        _pipeNetworks = pipeNetworks;
    }

    public SearchItemsCommand SearchItems()
    {
        return new SearchItemsCommand(new ItemIndex(_items));
    }

    public SearchRecipesCommand SearchRecipes()
    {
        return new SearchRecipesCommand(new RecipeIndex(_recipes));
    }

    public SearchStationpediaCommand SearchStationpedia()
    {
        return new SearchStationpediaCommand(new StationpediaIndex(_stationpedia));
    }

    public ReadStationpediaCommand ReadStationpedia()
    {
        return new ReadStationpediaCommand(_stationpedia);
    }

    public SearchInventoryCommand SearchInventory()
    {
        return new SearchInventoryCommand(new InventoryIndex(_player));
    }

    public DescribePlayerCommand DescribePlayer()
    {
        return new DescribePlayerCommand(_player);
    }

    public GetWorldCommand GetWorld()
    {
        return new GetWorldCommand(_worlds);
    }

    public ReadComputerCommand ReadComputer()
    {
        return new ReadComputerCommand(_icDevices);
    }

    public ReadHousingCommand ReadHousing()
    {
        return new ReadHousingCommand(_icDevices);
    }

    public WriteComputerCommand WriteComputer()
    {
        return new WriteComputerCommand(_icDevices);
    }

    public ImportChipCommand ImportChip()
    {
        return new ImportChipCommand(_icDevices);
    }

    public ExportChipCommand ExportChip()
    {
        return new ExportChipCommand(_icDevices);
    }

    public SelectHousingCommand SelectHousing()
    {
        return new SelectHousingCommand(_icDevices);
    }

    public ListInstructionsCommand ListInstructions()
    {
        return new ListInstructionsCommand(_mips);
    }

    public SearchStructuresCommand SearchStructures()
    {
        return new SearchStructuresCommand(_structures);
    }

    public DescribeStructureCommand DescribeStructure()
    {
        return new DescribeStructureCommand(_structureInspector, _structures);
    }

    public DescribeThingCommand DescribeThing()
    {
        return new DescribeThingCommand(_thingInspector, _structures, _things);
    }

    public SearchThingsCommand SearchThings()
    {
        return new SearchThingsCommand(_things, _player);
    }

    public ReadThingSlotsCommand ReadThingSlots()
    {
        return new ReadThingSlotsCommand(_slots);
    }

    public ReadRoomCommand ReadRoom()
    {
        return new ReadRoomCommand(_rooms, _player);
    }

    public ReadPipeNetworkCommand ReadPipeNetwork()
    {
        return new ReadPipeNetworkCommand(_pipeNetworks);
    }
}
