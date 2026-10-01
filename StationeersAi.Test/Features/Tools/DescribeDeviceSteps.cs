using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools
{
    [Binding]
    public sealed class DescribeDeviceSteps
    {
        private readonly ScenarioContext _context;

        public DescribeDeviceSteps(ScenarioContext context)
        {
            _context = context;
        }

        private World World => Hooks.GetWorld(_context);

        [Given("the device {string} exists")]
        public void GivenTheDeviceExists(string name)
        {
            World.Inspector.Details = new DeviceDetails(
                name,
                "StructureAirVent",
                101,
                7,
                true,
                new[] { new DeviceLogicValue("Pressure", 101.3, false), new DeviceLogicValue("On", 1, true) },
                new[] { new DeviceSlotInfo(0, "Filter", "empty") },
                null,
                null);
        }

        [Given("the housing {string} exists")]
        public void GivenTheHousingExists(string name)
        {
            World.Inspector.Details = new DeviceDetails(
                name,
                "StructureCircuitHousing",
                100,
                7,
                null,
                new DeviceLogicValue[0],
                new DeviceSlotInfo[0],
                new[]
                {
                    new HousingDeviceEntry("db", name, 100),
                    new HousingDeviceEntry("d0", "Heater", 101),
                    new HousingDeviceEntry("d1", "empty", 0),
                },
                null);
        }

        [Given("the housing {string} runs code")]
        public void GivenTheHousingRunsCode(string name)
        {
            GivenTheHousingExists(name);
            var details = World.Inspector.Details;
            World.Inspector.Details = new DeviceDetails(
                details.Name,
                details.Type,
                details.Thing,
                details.NetworkId,
                details.IsOn,
                details.Logic,
                details.Slots,
                details.Devices,
                new ChipState("IncorrectVariable", "12", 294, "6c96f2ae"));
        }

        [Given("no device matches")]
        public void GivenNoDeviceMatches()
        {
            World.Inspector.Details = null;
        }

        [When("I describe the device {string}")]
        public void WhenIDescribeTheDevice(string device)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "describe_device",
                new ToolArguments(new System.Collections.Generic.Dictionary<string, string> { ["device"] = device })));
        }
    }
}
