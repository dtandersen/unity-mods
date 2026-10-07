using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// One IC housing: the thing that holds an IC10 chip and the program burned
/// onto it. <see cref="IHousingRepository"/> builds these from a label or
/// thing id.
/// </summary>
public interface IHousing
{
    /// <summary>Reads the chip text. Never returns null.</summary>
    IcReadResult Read();
}
