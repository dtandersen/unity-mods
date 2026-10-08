namespace StationeersAi.Entity;

/// <summary>
/// One IC computer: its screen holds editor text, its dropdown picks one
/// housing on the network, and its buttons move text between the screen and
/// the housing chip. The buttons do what the game's do — press and hope:
/// <see cref="StationeersAi.Repository.IIcDevices"/> builds one from a thing
/// id, and callers judge the result by reading the entities afterwards,
/// which is all the game itself reports. Faults raise; refusals are the
/// caller's call. Implementations count lines as <c>code.Split('\n').Length</c>.
/// </summary>
public interface IComputer
{
    /// <summary>The label on the computer, as the stationeer reads it.</summary>
    string Name { get; }

    /// <summary>Writes code to the screen. Throws when the editor cannot take it.</summary>
    void Write(string code);

    /// <summary>Reads the screen text and its hash. Throws when the editor cannot be read.</summary>
    Program Read();

    /// <summary>
    /// Burns screen text to the chip of housing <paramref name="device"/>.
    /// Throws when the housing is not on this computer's network or the
    /// button cannot be pressed; whether the burn took is told by reading
    /// that housing afterwards.
    /// </summary>
    void Export(long device);

    /// <summary>
    /// Pulls the chip the dropdown points at into the screen, the game's
    /// own button: it takes no target, and refuses to guess when the
    /// dropdown points at nothing. Throws when the button cannot be pressed.
    /// </summary>
    void Import();

    /// <summary>Points the dropdown at housing <paramref name="device"/>. Throws when it cannot.</summary>
    void Select(long device);

    /// <summary>
    /// The thing id of the housing under the dropdown, or 0 when the dropdown
    /// points at none. Throws when the dropdown cannot be read.
    /// </summary>
    long Selected();
}
