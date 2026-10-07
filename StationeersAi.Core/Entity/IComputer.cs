namespace StationeersAi.Entity;

/// <summary>
/// One IC computer: its screen holds editor text, its dropdown picks one
/// housing on the network, and its import and export buttons move text
/// between the screen and the housing chip.
/// <see cref="StationeersAi.Repository.IIcDevices"/> builds one from a thing
/// id, which the stationeer reads from the device catalogue.
/// Implementations count lines as <c>code.Split('\n').Length</c>.
/// </summary>
public interface IComputer
{
    /// <summary>The label on the computer, as the stationeer reads it.</summary>
    string Name { get; }

    /// <summary>
    /// Writes code to the screen, and only while the chip under the dropdown
    /// still hashes to <paramref name="chipHash"/>. Refuses the write when the
    /// dropdown points at nothing, or at another program. Never returns null.
    /// </summary>
    IcWriteResult Write(string code, string chipHash);

    /// <summary>Reads the screen text and its hash. Throws when the editor cannot be read.</summary>
    Program Read();

    /// <summary>Burns screen text to the chip of housing <paramref name="device"/>, then checks the hash. Never returns null.</summary>
    IcWriteResult Export(long device);

    /// <summary>Pulls the chip text of housing <paramref name="device"/> into the screen. Never returns null.</summary>
    IcWriteResult Import(long device);

    /// <summary>Points the dropdown at housing <paramref name="device"/>. Never returns null.</summary>
    IcWriteResult Select(long device);

    /// <summary>
    /// The thing id of the housing under the dropdown, or 0 when the dropdown
    /// points at none. Throws when the dropdown cannot be read.
    /// </summary>
    long Selected();
}
