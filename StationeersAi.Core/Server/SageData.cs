using System.Collections.Generic;
using StationeersAi.Repository;

namespace StationeersAi.Server;

/// <summary>
/// All game data the Sage HTTP layer reads. The mod fills this from the
/// live game; tests fill it from in-memory fakes. Keeps the router free of
/// Unity types.
/// </summary>
public sealed class SageData
{
    public IItemRepository Items { get; set; } = null!;

    public IStructureRepository Structures { get; set; } = null!;

    public IRecipeRepository Recipes { get; set; } = null!;

    public IStationpediaRepository Stationpedia { get; set; } = null!;

    public IPlayerContext Player { get; set; } = null!;

    public IWorldContext Worlds { get; set; } = null!;

    public IIcEditor Ic { get; set; } = null!;

    public IIcExport Export { get; set; } = null!;

    public IMipsReference Mips { get; set; } = null!;

    public IDeviceCatalog Devices { get; set; } = null!;

    public IDeviceInspector Inspector { get; set; } = null!;

    public IThingCatalog Things { get; set; } = null!;

    public IThingContents ThingContents { get; set; } = null!;

    public IRoomInspector Rooms { get; set; } = null!;
}
