using System;
using StationeersAi.Window;
using Util.Commands;

namespace StationeersAi.Game;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai clear</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer streams into the Sage
/// window when the model replies. Builds the console router from the
/// command factory for each line it receives.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    private readonly CommandFactory _commands;
    private readonly SagePresenter _presenter;

    public AiConsoleCommand(CommandFactory commands, SagePresenter presenter)
    {
        _commands = commands;
        _presenter = presenter;
    }

    public override CommandScope Scope => CommandScope.InGame;

    static string subcommands="clear | compact | <question>";

    public override string HelpText =>
        $"Usage: ai {subcommands}";

    public override string[] Arguments => new[] { subcommands };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        return _commands.AiConsoleRouter(_presenter).Execute(args);
    }
}
