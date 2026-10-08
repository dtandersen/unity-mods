using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The computer whose screen takes the selected chip's text.</summary>
public sealed record ImportChipRequest(long Computer);

/// <summary>
/// Domain operation: pull the chip the dropdown has selected into the
/// computer's screen. The ask names no housing because the game's button
/// reads no housing: it takes what the dropdown has selected, and this
/// refuses when the dropdown selects nothing. The answer counts what landed
/// on the screen, read back after the button. A missed id becomes
/// <see cref="Status.NOT_FOUND"/>; a machine that fails outright raises.
/// </summary>
public sealed class ImportChipCommand : Command<ImportChipRequest, ComputerActionResult>
{
    private readonly IIcDevices _devices;

    public ImportChipCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ComputerActionResult Execute(ImportChipRequest request)
    {
        try
        {
            var target = _devices.FindComputer(request.Computer);
            if (target.Selected() == 0)
            {
                return ComputerAnswers.Refuse("The computer points at no housing. Pick the housing first.");
            }

            target.Import();
            var screen = target.Read();
            return ComputerAnswers.Accept(target.Name, ComputerAnswers.LineCount(screen.SourceCode));
        }
        catch (ThingNotFound notFound)
        {
            return ComputerAnswers.NotFound(notFound.Message);
        }
    }
}
