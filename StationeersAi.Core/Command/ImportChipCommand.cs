using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The housing to pull from and the computer whose screen takes the text.</summary>
public sealed record ImportChipRequest(long Computer, long Device);

/// <summary>
/// Domain operation: pull the chip text of one housing into the computer's
/// screen. The answer counts what ended up on the screen, read back after
/// the button. A missed id becomes <see cref="Status.NOT_FOUND"/>; a
/// machine that fails outright raises.
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
            target.Import(request.Device);
            var screen = target.Read();
            return ComputerAnswers.Accept(target.Name, ComputerAnswers.LineCount(screen.SourceCode));
        }
        catch (ThingNotFound notFound)
        {
            return ComputerAnswers.NotFound(notFound.Message);
        }
    }
}
