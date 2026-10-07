using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Tools;

[Binding]
public sealed class WorldToolsSteps
{
    private readonly ScenarioContext _context;

    public WorldToolsSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("a world")]
    public void GivenAWorld()
    {
        World.Worlds.Info = new WorldInfo(
            "Europa",
            "Europa1",
            -9.8f,
            12,
            false,
            false,
            "20m",
            "60d",
            72.0,
            0.01f,
            5.2,
            3.1f,
            false,
            500f,
            0.37f,
            0.35f,
            false,
            -20.0,
            0.0,
            new[] { new GasMixEntry("Nitrogen", 90.0), new GasMixEntry("Oxygen", 10.0) });
    }

    [Given("no world is loaded")]
    public void GivenNoWorldIsLoaded()
    {
        World.Worlds.Info = null;
    }
}
