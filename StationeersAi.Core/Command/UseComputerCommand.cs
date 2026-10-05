using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>
/// Domain operation: stage editor text, pull chip code, and burn it back.
/// Writes <paramref name="sourceCode"/> to the computer first when present,
/// pulls the housing chip into the editor when <paramref name="import"/> is
/// true, then exports only when <paramref name="export"/> is true. Mirrors
/// the monitor flow: source sets the editor, device picks the housing,
/// export and import move the text.
/// </summary>
public sealed class UseComputerCommand
{
    private readonly IIcEditor _editor;
    private readonly IIcExport _export;

    public UseComputerCommand(IIcEditor editor, IIcExport export)
    {
        _editor = editor;
        _export = export;
    }

    public IcWriteResult Execute(string? computer, string? device, string? sourceCode, bool export, bool import)
    {
        IcWriteResult? staged = null;
        if (sourceCode != null)
        {
            staged = _editor.Write(computer, sourceCode);
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

            staged = _export.Import(computer, device);
            if (staged == null || !staged.Written)
            {
                return staged ?? new IcWriteResult(false, null, 0, "Import fell through.");
            }
        }

        if (!export)
        {
            return staged ?? new IcWriteResult(false, null, 0, "Nothing to do: set sourceCode, import, export, or a mix.");
        }

        if (string.IsNullOrWhiteSpace(device))
        {
            return new IcWriteResult(false, null, 0, "device is required to export.");
        }

        return _export.Export(computer, device);
    }
}
