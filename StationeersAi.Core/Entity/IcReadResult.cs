namespace StationeersAi.Entity
{
    /// <summary>
    /// What's in a computer's editor: which device, its full text, or why the
    /// read did not happen.
    /// </summary>
    public sealed record IcReadResult(bool Found, string Device, string Code, string Error);
}
