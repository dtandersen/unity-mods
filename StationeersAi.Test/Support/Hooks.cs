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
        _context.Set(new WorldContext(), WorldKey);
    }

    [AfterScenario]
    public void DisposeWorld()
    {
        _context.Get<WorldContext>(WorldKey).Dispose();
    }

    public static WorldContext GetWorld(ScenarioContext context)
    {
        return context.Get<WorldContext>(WorldKey);
    }
}
