using Reqnroll;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using StationeersAi.Tool;

namespace StationeersAi.Test.Features.Tools;

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

    [Given("the gas sensor {string} reads 290K at 3.3 kPa")]
    public void GivenTheGasSensorReads(string name)
    {
        World.Inspector.Details = new DeviceDetails(
            name,
            "StructureGasSensor",
            1482,
            1194,
            null,
            new[]
            {
                new DeviceLogicValue("Pressure", 3.35518, false),
                new DeviceLogicValue("Temperature", 290.131, false),
                new DeviceLogicValue("RatioOxygen", 0.195957, false),
                new DeviceLogicValue("RatioCarbonDioxide", 0.774701, false),
                new DeviceLogicValue("RatioMethane", 0, false),
                new DeviceLogicValue("RatioWater", 0, false),
                new DeviceLogicValue("RatioNitrogen", 0.0241535, false),
            },
            new DeviceSlotInfo[0],
            null,
            null);
    }

    [Given("two devices share the name {string}")]
    public void GivenTwoDevicesShareTheName(string name)
    {
        World.Devices.Devices.Clear();
        World.Devices.Devices.Add(new DeviceInfo(name, "StructureGasSensor", 1482, 1194, true, 5));
        World.Devices.Devices.Add(new DeviceInfo(name, "StructureGasSensor", 1123, 1119, true, 5));
        World.Inspector.Details = null;
    }

    [When("I describe the device {string} with full {string}")]
    public void WhenIDescribeTheDeviceWithFull(string device, string full)
    {
        World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
            "call_1",
            "describe_device",
            new ToolArguments(new System.Collections.Generic.Dictionary<string, string> { ["device"] = device, ["full"] = full })));
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
