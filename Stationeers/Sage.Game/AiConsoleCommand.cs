using Sage.Window;
using Util.Commands;

namespace Sage.Game;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai reset</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer streams into the Sage
/// window when the model replies. The mod owns the presenter, so a command
/// issued before any level is loaded says so.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    private readonly SageMod _mod;

    public AiConsoleCommand(SageMod mod)
    {
        _mod = mod;
    }

    public override CommandScope Scope => CommandScope.InGame;

    static string subcommands="reset | compact | <question>";

    public override string HelpText =>
        $"Usage: ai {subcommands}";

    public override string[] Arguments => new[] { subcommands };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        return _mod.OnConsoleCommand(args);
    }
}
