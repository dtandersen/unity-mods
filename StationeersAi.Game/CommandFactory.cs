using System;
using StationeersAi.Command;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Window;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Builds game commands and the live readers behind them from the current
/// game sources. Every method builds fresh stateless objects, so callers
/// always read live data and the factory itself holds no game state.
/// </summary>
public sealed class CommandFactory
{
    private readonly IPrefabProvider _prefabs = new StationeersPrefabProvider();
    private readonly IHumanProvider _humans = new StationeersHumanProvider();
    private readonly IRecipeProvider _recipes = new StationeersRecipeProvider();
    private readonly IStationpediaProvider _stationpedia = new StationeersStationpediaProvider();
    private readonly IMainThread _executor;
    private readonly Logger _log;

    public CommandFactory(IMainThread executor, Logger log)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public IItemRepository Items()
    {
        return new StationeersItemRepository(_prefabs, _executor, _log);
    }

    public IStructureRepository Structures()
    {
        return new StationeersStructureRepository(_prefabs, _executor, _log);
    }

    public IRecipeRepository Recipes()
    {
        return new StationeersRecipeRepository(_recipes, _executor, _log);
    }

    public IStationpediaRepository Stationpedia()
    {
        return new StationeersStationpediaRepository(_stationpedia, _executor, _log);
    }

    public IPlayerContext Player()
    {
        return new StationeersPlayerContext(_humans, _executor, _log);
    }

    public IWorldContext Worlds()
    {
        return new StationeersWorldContext(_executor, _log);
    }

    public IIcEditor IcEditor()
    {
        return new StationeersIcEditor(_executor, _log);
    }

    public IIcExport IcExport()
    {
        return new StationeersIcExport(_executor, _log);
    }

    public IMipsReference Mips()
    {
        return new StationeersMipsReference(_log);
    }

    public IDeviceCatalog Devices()
    {
        return new StationeersDeviceCatalog(_executor, _log);
    }

    public IDeviceInspector Inspector()
    {
        return new StationeersDeviceInspector(_executor, _log);
    }

    public IThingCatalog Things()
    {
        return new StationeersThingCatalog(_executor, _log);
    }

    public IThingContents Contents()
    {
        return new StationeersThingContents(_executor, _log);
    }

    public IRoomInspector Rooms()
    {
        return new StationeersRoomInspector(_executor, _log);
    }

    public SearchItemsCommand SearchItems()
    {
        return new SearchItemsCommand(new ItemIndex(Items()));
    }

    public SearchStructuresCommand SearchStructures()
    {
        return new SearchStructuresCommand(new StructureIndex(Structures()));
    }

    public SearchRecipesCommand SearchRecipes()
    {
        return new SearchRecipesCommand(new RecipeIndex(Recipes()));
    }

    public SearchStationpediaCommand SearchStationpedia()
    {
        return new SearchStationpediaCommand(new StationpediaIndex(Stationpedia()));
    }

    public ReadStationpediaCommand ReadStationpedia()
    {
        return new ReadStationpediaCommand(Stationpedia());
    }

    public SearchInventoryCommand SearchInventory()
    {
        return new SearchInventoryCommand(new InventoryIndex(Player()));
    }

    public GetStatusCommand GetStatus()
    {
        return new GetStatusCommand(Player());
    }

    public GetWorldCommand GetWorld()
    {
        return new GetWorldCommand(Worlds());
    }

    public WriteIcCommand WriteIc()
    {
        return new WriteIcCommand(IcEditor());
    }

    public ReadIcCommand ReadIc()
    {
        return new ReadIcCommand(IcEditor());
    }

    public ExportIcCommand ExportIc()
    {
        return new ExportIcCommand(IcExport());
    }

    public ReadMipsCommand ReadMips()
    {
        return new ReadMipsCommand(Mips());
    }

    public SearchDevicesCommand SearchDevices()
    {
        return new SearchDevicesCommand(Devices());
    }

    public DescribeDeviceCommand DescribeDevice()
    {
        return new DescribeDeviceCommand(Inspector(), Devices());
    }

    public SearchThingsCommand SearchThings()
    {
        return new SearchThingsCommand(Things(), Player());
    }

    public ReadRoomCommand ReadRoom()
    {
        return new ReadRoomCommand(Rooms(), Player());
    }

    public AiConsoleRouter AiConsoleRouter(SagePresenter presenter)
    {
        return new AiConsoleRouter(presenter);
    }
}
