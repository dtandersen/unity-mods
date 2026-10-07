using StationeersAi.Formatting;

namespace StationeersAi.Entity;

/// <summary>In-memory IC housing: one chip holding one program.</summary>
public sealed class InMemoryHousing : IHousing
{
    /// <summary>The name reads report as the housing.</summary>
    public string Name { get; set; } = "Climate Control";

    /// <summary>The text reads return as the chip contents.</summary>
    public string ChipCode { get; set; } = string.Empty;

    public Program Read()
    {
        return new Program(ChipCode, CodeHash.Hash(ChipCode));
    }
}
