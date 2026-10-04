using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: export editor text to a housing chip.</summary>
public sealed class ExportIcCommand
{
    private readonly IIcExport _export;

    public ExportIcCommand(IIcExport export)
    {
        _export = export;
    }

    public IcWriteResult Execute(string? computer, string? device)
    {
        return _export.Export(computer, device);
    }
}
