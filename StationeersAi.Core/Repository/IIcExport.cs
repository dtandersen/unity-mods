using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// Exports a computer's editor text to a housing chip, then verifies the
/// burn by comparing code hashes.
/// </summary>
public interface IIcExport
{
    /// <summary>Exports editor text to the chip. Never returns null.</summary>
    IcWriteResult Export(string? computer, string? device);

    /// <summary>Pulls chip code into the editor. Never returns null.</summary>
    IcWriteResult Import(string? computer, string? device);

    /// <summary>Points the computer at one housing. Never returns null.</summary>
    IcWriteResult Select(string? computer, string? device);

    /// <summary>Reads the pointed housing. Never returns null.</summary>
    ReadComputerResult Selected(string? computer);
}
