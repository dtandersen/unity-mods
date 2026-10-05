using StationeersAi.Window;
using Util.Commands;

namespace StationeersAi.Game;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai reset</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer streams into the Sage
/// window when the model replies. Hands each line straight to the shared
/// presenter.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    private readonly SagePresenter _presenter;

    public AiConsoleCommand(SagePresenter presenter)
    {
        _presenter = presenter;
    }

    public override CommandScope Scope => CommandScope.InGame;

    static string subcommands="reset | compact | <question>";

    public override string HelpText =>
        $"Usage: ai {subcommands}";

    public override string[] Arguments => new[] { subcommands };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        return _presenter.OnConsoleCommand(args);
    }
}
