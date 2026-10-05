using StationeersAi.Repository;
using StationeersAi.Server;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Builds the live readers behind the core <see cref="SageCommands"/> from
/// the current game sources. Every method builds fresh stateless objects,
/// so callers always read live data and the factory itself holds no game
/// state.
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
        _executor = executor;
        _log = log;
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

    public IThingSlots Slots()
    {
        return new StationeersThingSlots(_executor, _log);
    }

    public IRoomInspector Rooms()
    {
        return new StationeersRoomInspector(_executor, _log);
    }

    /// <summary>
    /// One <see cref="SageCommands"/> wired with fresh live readers. The
    /// router and the chat assistant share this, so both serve the same data.
    /// </summary>
    public SageCommands Commands()
    {
        return new SageCommands(
            Items(),
            Structures(),
            Recipes(),
            Stationpedia(),
            Player(),
            Worlds(),
            IcEditor(),
            IcExport(),
            Mips(),
            Devices(),
            Inspector(),
            Things(),
            Slots(),
            Rooms());
    }
}
