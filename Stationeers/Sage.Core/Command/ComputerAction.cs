using System.Collections.Generic;

namespace Sage.Command;

/// <summary>
/// What the computer did after one button: the device that ended up holding
/// the text and how many lines it carries, or the reasons the button was
/// refused. Shared by the four IC button commands — write, import, export,
/// select.
/// </summary>
public sealed record ComputerActionResult(
    Status Status, List<string> Errors, string? Device, int Lines) : BaseResult(Status, Errors);

/// <summary>
/// The three shapes every button command hands back: an acceptance naming
/// the device, a refusal with its sentence, and a missed id.
/// </summary>
internal static class ComputerAnswers
{
    internal static int LineCount(string code)
    {
        return (code ?? string.Empty).Split('\n').Length;
    }

    internal static ComputerActionResult Accept(string device, int lines)
    {
        return new ComputerActionResult(Status.SUCCESS, new List<string>(), device, lines);
    }

    internal static ComputerActionResult Refuse(string message)
    {
        return new ComputerActionResult(Status.BAD_REQUEST, new List<string> { message }, null, 0);
    }

    internal static ComputerActionResult NotFound(string message)
    {
        return new ComputerActionResult(Status.NOT_FOUND, new List<string> { message }, null, 0);
    }
}
