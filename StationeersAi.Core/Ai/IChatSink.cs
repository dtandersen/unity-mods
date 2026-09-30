namespace StationeersAi.Ai
{
    /// <summary>
    /// Where the assistant writes chat lines. The plugin adapts Stationeers'
    /// console window; the specs record the lines instead.
    /// </summary>
    public interface IChatSink
    {
        /// <summary>Writes one already-formatted line to the chat.</summary>
        void Write(string line);
    }
}
