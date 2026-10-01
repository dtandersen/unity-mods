using System.Collections.Generic;
using Reqnroll;
using Shouldly;
using StationeersAi.Entity;
using StationeersAi.Test.Support;
using StationeersAi.Tool;

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
            World.Devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7, true));
            World.Devices.Devices.Add(new DeviceInfo("Gas Sensor", "StructureGasSensor", 102, 0, true));
            World.Devices.Devices.Add(new DeviceInfo("Iron Pipe", "StructurePipe", 103, 0, false));
        }
        [When("I search devices on network {string}")]
        public void WhenISearchDevicesOnNetwork(string network)
        {
            Search(network, null);
        }

        [When("I search devices for {string} on network {string}")]
        public void WhenISearchDevicesForOnNetwork(string query, string network)
        {
            Search(network, query);
        }

        [Then("the search used network {string}")]
        public void ThenTheSearchUsedNetwork(string expected)
        {
            World.Devices.LastNetwork.ShouldBe(long.Parse(expected));
        }
        [When("I search devices with logic {string}")]
        public void WhenISearchDevicesWithLogic(string logic)
        {
            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "search_devices",
                new ToolArguments(new Dictionary<string, string> { ["logic"] = logic })));
        }

        private void Search(string network, string query)
        {
            var values = new Dictionary<string, string>();
            if (network != null)
            {
                values["network"] = network;
            }

            if (query != null)
            {
                values["query"] = query;
            }

            World.ToolResult = TestComposition.Tools(World).Handle(new ToolCall(
                "call_1",
                "search_devices",
                new ToolArguments(values)));
        }
    }
}
