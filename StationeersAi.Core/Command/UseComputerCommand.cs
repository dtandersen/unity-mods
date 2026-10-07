using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>
/// Domain operation: stage editor text, pull chip code, and burn it back.
/// Resolves the named computer first, so every action works on that one
/// machine. Writes <paramref name="sourceCode"/> to the screen when present,
/// pulls the housing chip into the editor when <paramref name="import"/> is
/// true, then exports only when <paramref name="export"/> is true. Mirrors
/// the monitor flow: source sets the screen, device picks the housing,
/// export and import move the text.
/// </summary>
public sealed class UseComputerCommand
{
    private readonly IComputerRepository _computers;

    public UseComputerCommand(IComputerRepository computers)
    {
        _computers = computers;
    }

    public IcWriteResult Execute(string? computer, string? device, string? sourceCode, bool export, bool import)
    {
        var found = _computers.Find(computer);
        var target = found.Computer;
        if (target == null)
        {
            return new IcWriteResult(false, null, 0, found.Error ?? "Computer not found.");
        }

        IcWriteResult? staged = null;
        if (sourceCode != null)
        {
            staged = target.Write(sourceCode);
            if (staged == null || !staged.Written)
            {
                return staged ?? new IcWriteResult(false, null, 0, "Write fell through.");
            }
        }

        if (import)
        {
            if (string.IsNullOrWhiteSpace(device))
            {
                return new IcWriteResult(false, null, 0, "device is required to import.");
            }

            staged = target.Import(device);
            if (staged == null || !staged.Written)
            {
                return staged ?? new IcWriteResult(false, null, 0, "Import fell through.");
            }
        }

        if (export)
        {
            if (string.IsNullOrWhiteSpace(device))
            {
                return new IcWriteResult(false, null, 0, "device is required to export.");
            }

            return target.Export(device);
        }

        if (staged == null && device != null)
        {
            return target.Select(device);
        }

        return staged ?? new IcWriteResult(false, null, 0, "Nothing to do: set sourceCode, device, import, export, or a mix.");
    }
}
