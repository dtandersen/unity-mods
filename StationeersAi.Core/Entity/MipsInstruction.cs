namespace StationeersAi.Entity
{
    /// <summary>One IC10 instruction: its mnemonic, usage, and what it does.</summary>
    public sealed record MipsInstruction(string Name, string Example, string Description);
}
