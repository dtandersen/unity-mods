using System.Collections.Generic;
using System.Globalization;
using Reqnroll;
using Sage.Entity;
using Sage.Test.Support;
using Caev.Tools;

namespace Sage.Test.Features.Tools;

[Binding]
public sealed class PipeNetworkToolsSteps
{
    private readonly ScenarioContext _context;

    public PipeNetworkToolsSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("a {word} pipe network {int} at {word} C and {word} kPa in {int} L holding:")]
    public void GivenAPipeNetworkHolding(string content, int network, string temperature, string pressure, int volume, Table gases)
    {
        var amounts = new List<GasMoles>();
        double total = 0;
        foreach (var row in gases.Rows)
        {
            var moles = double.Parse(row["Moles"], CultureInfo.InvariantCulture);
            amounts.Add(new GasMoles(row["MoleType"], moles));
            total += moles;
        }

        World.PipeNetworks.Add(new PipeNetworkDetails(
            network,
            content,
            double.Parse(temperature, CultureInfo.InvariantCulture),
            double.Parse(pressure, CultureInfo.InvariantCulture),
            volume,
            total,
            amounts,
            new[] { "Liquid Pipe Straight #21798" }));
    }

    [Given("device {string} joins pipe network {int}")]
    public void GivenDeviceJoinsPipeNetwork(string device, int network)
    {
        World.PipeNetworks.Join(device, network);
    }

    [When("I call the {string} tool with pipenetwork {string}")]
    public void WhenICallTheToolWithPipeNetwork(string tool, string network)
    {
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new ToolCall("call_1", tool, new ToolArguments(
                new Dictionary<string, string> { ["pipenetwork"] = network })));
    }

    [When("I call the {string} tool with device {string}")]
    public void WhenICallTheToolWithDevice(string tool, string device)
    {
        World.ToolResult = TestComposition.Tools(World)
            .Handle(new ToolCall("call_1", tool, new ToolArguments(
                new Dictionary<string, string> { ["device"] = device })));
    }
}
