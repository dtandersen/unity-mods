using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Tools
{
    [Binding]
    public sealed class DeviceSearchSteps
    {
        private readonly ScenarioContext _context;

        public DeviceSearchSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("the world has devices")]
        public void GivenTheWorldHasDevices()
        {
            World.Devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7));
            World.Devices.Devices.Add(new DeviceInfo("Gas Sensor", "StructureGasSensor", 102, 0));
        }
    }
}
