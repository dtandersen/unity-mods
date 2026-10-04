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
    public IItemRepository Items { get; set; }

    public IStructureRepository Structures { get; set; }

    public IRecipeRepository Recipes { get; set; }

    public IStationpediaRepository Stationpedia { get; set; }

    public IPlayerContext Player { get; set; }

    public IWorldContext Worlds { get; set; }

    public IIcEditor Ic { get; set; }

    public IIcExport Export { get; set; }

    public IMipsReference Mips { get; set; }

    public IDeviceCatalog Devices { get; set; }

    public IDeviceInspector Inspector { get; set; }

    public IThingCatalog Things { get; set; }

    public IThingContents ThingContents { get; set; }

    public IRoomInspector Rooms { get; set; }
}
