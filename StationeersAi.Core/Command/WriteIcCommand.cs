using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: write MIPS source to a computer's editor.</summary>
    public sealed class WriteIcCommand
    {
        private readonly IIcEditor _editor;

        public WriteIcCommand(IIcEditor editor)
        {
            _editor = editor;
        }

        public IcWriteResult Execute(string computerName, long? computerThing, string code)
        {
            return _editor.Write(computerName, computerThing, code);
        }
    }
}
