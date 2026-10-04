using System;
using StationeersAi.Command;

namespace StationeersAi.Game;

/// <summary>
/// Builds the console commands. The mod func is resolved per call because
/// the shell rebuilds the mod every level.
/// </summary>
public sealed class CommandFactory
{
    private readonly Func<SageMod?> _mod;

    public CommandFactory(Func<SageMod?> mod)
    {
        _mod = mod ?? throw new ArgumentNullException(nameof(mod));
    }

    public AiConsoleRouter AiConsoleRouter()
    {
        return new AiConsoleRouter(() => _mod()?.ConsolePresenter());
    }
}
