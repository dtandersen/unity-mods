namespace StationeersAi.Entity
{
    /// <summary>
    /// The outcome of an editor write: which device received the code and how
    /// many lines it holds, or why the write did not happen.
    /// </summary>
    public sealed record IcWriteResult(bool Written, string Device, int Lines, string Error);
}
