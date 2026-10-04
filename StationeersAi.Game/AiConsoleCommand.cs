using System;
using Util.Commands;

namespace StationeersAi.Game;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai clear</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer streams into the Sage
/// window when the model replies. The mod is resolved per call because the
/// presenter only exists while the mod is loaded.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    private readonly CommandFactory _commandFactory;

    public AiConsoleCommand(CommandFactory commandFactory)
    {
        _commandFactory = commandFactory;
    }

    public override CommandScope Scope => CommandScope.InGame;

    static string subcommands="clear | compact | <question>";

    public override string HelpText =>
        $"Usage: ai {subcommands}";

    public override string[] Arguments => new[] { subcommands };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        var command = _commandFactory.AiConsoleRouter();

        return command.Execute(args);
    }
}
