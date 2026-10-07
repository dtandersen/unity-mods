using System;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>
/// Domain operation: stage editor text, pull chip code, and burn it back.
/// Resolves the computer by thing id first, so every action works on that one
/// machine. Writes <paramref name="sourceCode"/> to the screen only after the
/// chip under the dropdown hashes to <paramref name="chipHash"/>, pulls the
/// housing chip into the editor when <paramref name="import"/> is true, then
/// exports only when <paramref name="export"/> is true. Mirrors the monitor
/// flow: the hash names the chip you mean, source sets the screen, device
/// picks the housing, export and import move the text.
/// </summary>
public sealed class UseComputerCommand
{
    private readonly IIcDevices _devices;

    public UseComputerCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public IcWriteResult Execute(long computer, long? device, string? sourceCode, string? chipHash, bool export, bool import)
    {
        IComputer target;
        try
        {
            target = _devices.FindComputer(computer);
        }
        catch (ThingNotFound notFound)
        {
            return new IcWriteResult(false, null, 0, notFound.Message);
        }
        catch (Exception)
        {
            return new IcWriteResult(false, null, 0, "Computer lookup failed.");
        }

        IcWriteResult? staged = null;
        if (sourceCode != null)
        {
            if (chipHash is null || string.IsNullOrWhiteSpace(chipHash))
            {
                return new IcWriteResult(false, null, 0, "A chip hash is required to write: name the hash of the chip under the dropdown.");
            }

            staged = target.Write(sourceCode, chipHash.Trim());
            if (staged == null || !staged.Written)
            {
                return staged ?? new IcWriteResult(false, null, 0, "Write fell through.");
            }
        }

        if (import)
        {
            if (!device.HasValue)
            {
                return new IcWriteResult(false, null, 0, "device is required to import.");
            }

            staged = target.Import(device.Value);
            if (staged == null || !staged.Written)
            {
                return staged ?? new IcWriteResult(false, null, 0, "Import fell through.");
            }
        }

        if (export)
        {
            if (!device.HasValue)
            {
                return new IcWriteResult(false, null, 0, "device is required to export.");
            }

            return target.Export(device.Value);
        }

        if (staged == null && device.HasValue)
        {
            return target.Select(device.Value);
        }

        return staged ?? new IcWriteResult(false, null, 0, "Nothing to do: set sourceCode, device, import, export, or a mix.");
    }
}
