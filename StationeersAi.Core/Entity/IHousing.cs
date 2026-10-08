namespace StationeersAi.Entity;

/// <summary>
/// One IC housing: the thing that holds an IC10 chip and the program burned
/// onto it. <see cref="StationeersAi.Repository.IIcDevices"/> builds it from
/// a thing id.
/// </summary>
public interface IHousing
{
    /// <summary>The label on the housing, as the stationeer reads it.</summary>
    string Name { get; }

    /// <summary>
    /// What the chip says is wrong with the program burned on it, such as
    /// "illegal opcode at line 3", or null when the chip compiles clean.
    /// </summary>
    string? CompileError { get; }

    /// <summary>Reads the program burned on the chip. Throws when the chip cannot be read.</summary>
    Program Read();
}
