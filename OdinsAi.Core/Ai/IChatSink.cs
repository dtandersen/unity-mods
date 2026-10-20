namespace OdinsAi.Ai
{
    /// <summary>
    /// Where the assistant writes chat lines. The plugin adapts Valheim's
    /// <c>Terminal</c>; the specs record the lines instead.
    /// </summary>
    public interface IChatSink
    {
        /// <summary>Writes one already-formatted line to the chat.</summary>
        void Write(string line);
    }
}