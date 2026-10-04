using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// The IC editor on a computer: accepts MIPS source for a named computer.
/// Implementations count lines as <c>code.Split('\n').Length</c>.
/// </summary>
public interface IIcEditor
{
    /// <summary>Writes code to the computer's editor. Never returns null.</summary>
    IcWriteResult Write(string? computer, string code);

    /// <summary>Reads the computer's editor text. Never returns null.</summary>
    IcReadResult Read(string? computer);
}
