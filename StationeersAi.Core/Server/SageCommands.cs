using StationeersAi.Command;
using StationeersAi.Query;
using StationeersAi.Repository;

namespace StationeersAi.Server;

/// <summary>
/// Builds a fresh command per call from the given readers. The game fills
/// this from live sources; tests fill it from in-memory fakes. The router
/// and the chat assistant read through the same object, so both serve the
/// same data. Holds no game state.
/// </summary>
public sealed class SageCommands
{
    private readonly IItemRepository _items;
    private readonly IStructureRepository _structures;
    private readonly IRecipeRepository _recipes;
    private readonly IStationpediaRepository _stationpedia;
    private readonly IPlayerContext _player;
    private readonly IWorldContext _worlds;
    private readonly IIcEditor _ic;
    private readonly IIcExport _export;
    private readonly IMipsReference _mips;
    private readonly IDeviceCatalog _devices;
    private readonly IDeviceInspector _inspector;
    private readonly IThingCatalog _things;
    private readonly IThingSlots _slots;
    private readonly IRoomInspector _rooms;

    public SageCommands(
        IItemRepository items,
        IStructureRepository structures,
        IRecipeRepository recipes,
        IStationpediaRepository stationpedia,
        IPlayerContext player,
        IWorldContext worlds,
        IIcEditor ic,
        IIcExport export,
        IMipsReference mips,
        IDeviceCatalog devices,
        IDeviceInspector inspector,
        IThingCatalog things,
        IThingSlots slots,
        IRoomInspector rooms)
    {
        _items = items;
        _structures = structures;
        _recipes = recipes;
        _stationpedia = stationpedia;
        _player = player;
        _worlds = worlds;
        _ic = ic;
        _export = export;
        _mips = mips;
        _devices = devices;
        _inspector = inspector;
        _things = things;
        _slots = slots;
        _rooms = rooms;
    }

    public SearchItemsCommand SearchItems()
    {
        return new SearchItemsCommand(new ItemIndex(_items));
    }

    public SearchStructuresCommand SearchStructures()
    {
        return new SearchStructuresCommand(new StructureIndex(_structures));
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

    public GetPlayerCommand GetPlayer()
    {
        return new GetPlayerCommand(_player);
    }

    public GetWorldCommand GetWorld()
    {
        return new GetWorldCommand(_worlds);
    }

    public WriteIcCommand WriteIc()
    {
        return new WriteIcCommand(_ic);
    }

    public ReadIcCommand ReadIc()
    {
        return new ReadIcCommand(_ic);
    }

    public ExportIcCommand ExportIc()
    {
        return new ExportIcCommand(_export);
    }

    public ReadInstructionsCommand ReadInstructions()
    {
        return new ReadInstructionsCommand(_mips);
    }

    public SearchDevicesCommand SearchDevices()
    {
        return new SearchDevicesCommand(_devices);
    }

    public DescribeDeviceCommand DescribeDevice()
    {
        return new DescribeDeviceCommand(_inspector, _devices);
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
}
