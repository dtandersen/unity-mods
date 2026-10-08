using System;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The screen to fill and the hash that names the chip you mean.</summary>
public sealed record WriteComputerRequest(long Computer, string Code, string? ChipHash);

/// <summary>
/// Domain operation: put text on the computer's screen. It lands only when
/// the named hash is the hash of the chip under the dropdown, so a write
/// aimed at one chip cannot overwrite the screen meant for another. A
/// missed id becomes <see cref="Status.NOT_FOUND"/>, a guard refusal
/// <see cref="Status.BAD_REQUEST"/>; a machine that fails outright raises.
/// </summary>
public sealed class WriteComputerCommand : Command<WriteComputerRequest, ComputerActionResult>
{
    private readonly IIcDevices _devices;

    public WriteComputerCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ComputerActionResult Execute(WriteComputerRequest request)
    {
        try
        {
            var target = _devices.FindComputer(request.Computer);
            if (request.ChipHash is null || string.IsNullOrWhiteSpace(request.ChipHash))
            {
                return ComputerAnswers.Refuse("A chip hash is required to write: name the hash of the chip under the dropdown.");
            }

            var selectedId = target.Selected();
            var selected = selectedId == 0 ? null : _devices.FindHousing(selectedId);
            var refusal = WriteGuard.Refusal(
                selected?.Name,
                selected == null ? string.Empty : selected.Read().Hash,
                request.ChipHash.Trim());
            if (refusal != null)
            {
                return ComputerAnswers.Refuse(refusal);
            }

            target.Write(request.Code);
            return ComputerAnswers.Accept(target.Name, ComputerAnswers.LineCount(request.Code));
        }
        catch (ThingNotFound notFound)
        {
            return ComputerAnswers.NotFound(notFound.Message);
        }
    }
}
