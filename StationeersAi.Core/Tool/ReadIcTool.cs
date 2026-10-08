using System;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Repository;
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

        // Commands answer with results. A fault still escaping one becomes
        // a plain failure for the model, not a dead turn.
        ReadComputerResult viewed;
        try
        {
            viewed = _computer.Execute(computerId);
        }
        catch (Exception)
        {
            return new ToolFailure("Computer read failed.");
        }

        if (viewed.Status != Status.SUCCESS)
        {
            return new ToolFailure(viewed.Errors.Count > 0 ? string.Join(" ", viewed.Errors) : "Computer read failed.");
        }

        var text = string.IsNullOrEmpty(viewed.SourceCode)
            ? "(editor is empty)"
            : viewed.SourceCode + $"\nCode hash: {viewed.Hash}";

        var device = arguments.Get("device");
        if (!string.IsNullOrWhiteSpace(device))
        {
            long deviceId;
            if (!ThingId.TryParse(device, out deviceId))
            {
                return new ToolFailure(ThingId.NotAThingId("device", device));
            }

            Program chip;
            try
            {
                var answer = _housing.Execute(new ReadHousingRequest(deviceId));
                if (answer.Status != Status.SUCCESS)
                {
                    return new ToolFailure(ToolAnswer.Line(answer, "Housing read failed."));
                }

                chip = answer.Program!;
            }
            catch (Exception)
            {
                return new ToolFailure("Housing read failed.");
            }

            text += "\n\n" + (string.IsNullOrEmpty(chip.SourceCode)
                ? $"Housing {deviceId} chip:\n(chip is empty)"
                : $"Housing {deviceId} chip:\n{chip.SourceCode}\nCode hash: {chip.Hash}");
        }

        return new ToolSuccess<ReadComputerResult>(viewed, text);
    }
}
