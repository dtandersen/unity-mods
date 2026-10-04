using System;
using StationeersAi.Command;
using StationeersAi.Window;

namespace StationeersAi.Game;

/// <summary>
/// Builds the console commands from the permanent window presenter.
/// </summary>
public sealed class CommandFactory
{
    private readonly SagePresenter _presenter;

    public CommandFactory(SagePresenter presenter)
    {
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
    }

    public AiConsoleRouter AiConsoleRouter()
    {
        return new AiConsoleRouter(_presenter);
    }
}
