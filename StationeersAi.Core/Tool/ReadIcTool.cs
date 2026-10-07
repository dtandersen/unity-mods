using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: read a computer's editor text and, when asked, a housing's burned chip. Never writes.</summary>
public sealed class ReadIcTool : ITool
{
    private readonly ReadComputerCommand _computer;
    private readonly ReadHousingCommand _housing;

    public ReadIcTool(ReadComputerCommand computer, ReadHousingCommand housing)
    {
        _computer = computer;
        _housing = housing;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "read_ic",
        "Read the full text in a computer or laptop's IC editor, and optionally a housing's burned chip. Takes the computer's thing id, with an optional housing thing id. Never writes.",
        new[]
        {
            new ToolParameter("computer", "The computer or laptop, by thing id. Read it from search_devices."),
            new ToolParameter("device", "The housing holding the chip, by thing id. Omit it to read only the editor.", false),
        });

    public ToolResult Execute(ToolArguments arguments)
    {
        var computer = arguments.Get("computer");
        if (string.IsNullOrWhiteSpace(computer))
        {
            return new ToolFailure("Missing required argument 'computer'.");
        }

        long computerId;
        if (!ThingId.TryParse(computer, out computerId))
        {
            return new ToolFailure(ThingId.NotAThingId("computer", computer));
        }

        var viewed = _computer.Execute(computerId);
        if (viewed == null || viewed.Computer == null)
        {
            return new ToolFailure(viewed?.Error ?? "Computer not found.");
        }

        var text = string.IsNullOrEmpty(viewed.SourceCode)
            ? "(editor is empty)"
            : viewed.SourceCode + $"\nCode hash: {CodeHash.Hash(viewed.SourceCode)}";

        var device = arguments.Get("device");
        if (!string.IsNullOrWhiteSpace(device))
        {
            long deviceId;
            if (!ThingId.TryParse(device, out deviceId))
            {
                return new ToolFailure(ThingId.NotAThingId("device", device));
            }

            var chip = _housing.Execute(deviceId);
            if (chip == null || !chip.Found)
            {
                return new ToolFailure(chip?.Error ?? "Housing not found.");
            }

            text += "\n\n" + (string.IsNullOrEmpty(chip.Code)
                ? $"Housing {chip.Device} chip:\n(chip is empty)"
                : $"Housing {chip.Device} chip:\n{chip.Code}\nCode hash: {CodeHash.Hash(chip.Code)}");
        }

        return new ToolSuccess<ReadComputerResult>(viewed, text);
    }
}
