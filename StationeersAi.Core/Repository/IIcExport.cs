using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Exports a computer's editor text to a housing chip, then verifies the
    /// burn by comparing code hashes.
    /// </summary>
    public interface IIcExport
    {
        /// <summary>Exports editor text to the chip. Never returns null.</summary>
        IcWriteResult Export(string computer, string device);
    }
}
