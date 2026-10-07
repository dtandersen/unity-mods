using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// One IC computer: its screen holds editor text, its dropdown picks one
/// housing on the network, and its import and export buttons move text
/// between the screen and the housing chip. <see cref="IComputerRepository"/>
/// builds these from a thing id. Housings are named by thing id too, so the
/// stationeer must read <see cref="IDeviceCatalog"/> to find one.
/// Implementations count lines as <c>code.Split('\n').Length</c>.
/// </summary>
public interface IComputer
{
    /// <summary>Writes code to the screen. Never returns null.</summary>
    IcWriteResult Write(string code);

    /// <summary>Reads the screen text. Never returns null.</summary>
    IcReadResult Read();

    /// <summary>Burns screen text to the chip of housing <paramref name="device"/>, then checks the hash. Never returns null.</summary>
    IcWriteResult Export(long device);

    /// <summary>Pulls the chip text of housing <paramref name="device"/> into the screen. Never returns null.</summary>
    IcWriteResult Import(long device);

    /// <summary>Points the dropdown at housing <paramref name="device"/>. Never returns null.</summary>
    IcWriteResult Select(long device);

    /// <summary>Reads the housing the dropdown points at. Never returns null.</summary>
    ReadComputerResult Selected();
}
