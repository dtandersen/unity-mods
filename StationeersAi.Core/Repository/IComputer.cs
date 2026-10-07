using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// One IC computer: its screen holds editor text, its dropdown picks one
/// housing on the network, and its import and export buttons move text
/// between the screen and the housing chip. <see cref="IComputerRepository"/>
/// builds these from a label or thing id. Implementations count lines as
/// <c>code.Split('\n').Length</c>.
/// </summary>
public interface IComputer
{
    /// <summary>Writes code to the screen. Never returns null.</summary>
    IcWriteResult Write(string code);

    /// <summary>Reads the screen text. Never returns null.</summary>
    IcReadResult Read();

    /// <summary>Burns screen text to the chip, then checks the hash. Never returns null.</summary>
    IcWriteResult Export(string? device);

    /// <summary>Pulls chip text into the screen. Never returns null.</summary>
    IcWriteResult Import(string? device);

    /// <summary>Points the computer at one housing. Never returns null.</summary>
    IcWriteResult Select(string? device);

    /// <summary>Reads the housing the dropdown points at. Never returns null.</summary>
    ReadComputerResult Selected();
}
