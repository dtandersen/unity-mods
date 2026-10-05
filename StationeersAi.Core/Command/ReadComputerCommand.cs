using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read a computer's editor text and housing.</summary>
public sealed class ReadComputerCommand
{
    private readonly IIcEditor _editor;
    private readonly IIcExport _export;

    public ReadComputerCommand(IIcEditor editor, IIcExport export)
    {
        _editor = editor;
        _export = export;
    }

    public ReadComputerResult Execute(string? computer)
    {
        var code = _editor.Read(computer);
        if (code == null || !code.Found)
        {
            return new ReadComputerResult(null, null, null, code?.Error ?? "Computer not found.");
        }

        var pointed = _export.Selected(computer);
        return new ReadComputerResult(code.Device, pointed?.Device, code.Code, pointed?.Error);
    }
}
