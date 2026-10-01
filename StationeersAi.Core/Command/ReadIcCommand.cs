using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: read a computer's editor text.</summary>
    public sealed class ReadIcCommand
    {
        private readonly IIcEditor _editor;

        public ReadIcCommand(IIcEditor editor)
        {
            _editor = editor;
        }

        public IcReadResult Execute(string computerName, long? computerThing)
        {
            return _editor.Read(computerName, computerThing);
        }
    }
}
