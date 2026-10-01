using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>
    /// The IC editor on a computer: accepts MIPS source for a named device.
    /// Implementations count lines as <c>code.Split('\n').Length</c>.
    /// </summary>
    public interface IIcEditor
    {
        /// <summary>Writes code to the device's editor. Never returns null.</summary>
        IcWriteResult Write(string device, string code);
    }
}
