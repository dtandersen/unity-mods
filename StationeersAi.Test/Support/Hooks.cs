using Reqnroll;

namespace StationeersAi.Test.Support;

[Binding]
public sealed class Hooks
{
    private const string WorldKey = "world";

    private readonly ScenarioContext _context;

    public Hooks(ScenarioContext context)
    {
        _context = context;
    }

    [BeforeScenario]
    public void CreateWorld()
    {
        _context.Set(new World(), WorldKey);
    }

    [AfterScenario]
    public void DisposeWorld()
    {
        _context.Get<World>(WorldKey).Dispose();
    }

    public static World GetWorld(ScenarioContext context)
    {
        return context.Get<World>(WorldKey);
    }
}
