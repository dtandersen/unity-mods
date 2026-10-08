using StationeersAi.Formatting;

namespace StationeersAi.Entity;

/// <summary>In-memory IC housing: one chip holding one program.</summary>
public sealed class InMemoryHousing : IHousing
{
    /// <summary>The thing id lookups answer to.</summary>
    public long Thing { get; set; } = 2200;

    /// <summary>The label reads report as the housing.</summary>
    public string Name { get; set; } = "Climate Control";

    /// <summary>The text reads return as the chip contents.</summary>
    public string ChipCode { get; set; } = string.Empty;

    /// <summary>What the chip wrongly reports about its program, or null when it compiles.</summary>
    public string? CompileError { get; set; }

    public Program Read()
    {
        return new Program(ChipCode, CodeHash.Hash(ChipCode));
    }
}
