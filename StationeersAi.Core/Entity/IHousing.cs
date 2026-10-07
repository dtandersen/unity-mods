namespace StationeersAi.Entity;

/// <summary>
/// One IC housing: the thing that holds an IC10 chip and the program burned
/// onto it. <see cref="StationeersAi.Repository.IIcDevices"/> builds it from
/// a thing id.
/// </summary>
public interface IHousing
{
    /// <summary>Reads the program burned on the chip. Throws when the chip cannot be read.</summary>
    Program Read();
}
