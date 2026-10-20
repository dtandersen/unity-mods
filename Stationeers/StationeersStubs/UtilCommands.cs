// Minimal Util.Commands fakes (real: Assembly-CSharp Util.Commands).
namespace Util.Commands
{
    public enum CommandScope { InGame, Global, Menu }

    public abstract class CommandBase
    {
        public abstract CommandScope Scope { get; }
        public abstract string HelpText { get; }
        public abstract string[] Arguments { get; }
        public abstract bool IsLaunchCmd { get; }
        public abstract string Execute(string[] args);
    }

    public static class CommandLine
    {
        public static void AddCommand(string name, CommandBase command) { }
    }
}
