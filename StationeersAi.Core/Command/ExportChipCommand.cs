using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The computer holding the text and the housing to burn it onto.</summary>
public sealed record ExportChipRequest(long Computer, long Device);

/// <summary>
/// Domain operation: burn the computer's screen text onto one housing's
/// chip and believe it only when the chip reads back clean: nothing on the
/// screen refuses the burn, the chip reports no compile error, and its hash
/// matches the editor. A missed id becomes <see cref="Status.NOT_FOUND"/>,
/// a failed burn <see cref="Status.BAD_REQUEST"/> with what the chip said;
/// a machine that fails outright raises.
/// </summary>
public sealed class ExportChipCommand : Command<ExportChipRequest, ComputerActionResult>
{
    private readonly IIcDevices _devices;

    public ExportChipCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ComputerActionResult Execute(ExportChipRequest request)
    {
        try
        {
            var target = _devices.FindComputer(request.Computer);
            var editor = target.Read();
            if (string.IsNullOrEmpty(editor.SourceCode))
            {
                return ComputerAnswers.Refuse("Editor is empty; nothing to export.");
            }

            target.Export(request.Device);
            var housing = _devices.FindHousing(request.Device);
            var error = housing.CompileError;
            if (error != null)
            {
                return ComputerAnswers.Refuse($"Chip reports {error}.");
            }

            if (housing.Read().Hash != editor.Hash)
            {
                return ComputerAnswers.Refuse("Chip hash does not match the editor.");
            }

            return ComputerAnswers.Accept(housing.Name, ComputerAnswers.LineCount(editor.SourceCode));
        }
        catch (ThingNotFound notFound)
        {
            return ComputerAnswers.NotFound(notFound.Message);
        }
    }
}
