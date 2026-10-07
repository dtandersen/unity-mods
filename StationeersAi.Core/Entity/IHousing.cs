namespace StationeersAi.Entity;

/// <summary>
/// One IC housing: the thing that holds an IC10 chip and the program burned
/// onto it. <see cref="StationeersAi.Repository.IIcDevices"/> builds it from
/// a thing id.
/// </summary>
public interface IHousing
{
    /// <summary>Reads the chip text. Never returns null.</summary>
    IcReadResult Read();
}
