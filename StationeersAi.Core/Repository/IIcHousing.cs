using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Reads burned code from IC housings. Never returns null.</summary>
public interface IIcHousing
{
    /// <summary>Reads the chip text of one housing.</summary>
    IcReadResult Read(string? device);
}
