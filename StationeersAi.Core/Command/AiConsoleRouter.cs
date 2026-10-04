using System;
using StationeersAi.Window;

namespace StationeersAi.Command;

/// <summary>
/// The <c>ai</c> console line, minus the shell: <c>ai &lt;question&gt;</c> asks
/// the assistant, <c>ai clear</c> drops the conversation, <c>ai compact</c>
/// summarises it. Lives in core so tests can drive the true parsing without
/// the game assemblies; the game-side command hands it the presenter.
/// </summary>
public sealed class AiConsoleRouter
{
    private readonly SagePresenter _presenter;

    public AiConsoleRouter(SagePresenter presenter)
    {
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
    }

    /// <summary>Runs one console line split into words. Never throws.</summary>
    public string Execute(string[] args)
    {
        var presenter = _presenter;

        if (args != null && args.Length > 0)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "window":
                    presenter.ShowAndFocus();
                    return "Sage window opened.";

                case "clear":
                case "reset":
                    presenter.ClearConversation();
                    return "Context cleared.";

                case "compact":
                    _ = presenter.CompactConversation();
                    return "Compacting context...";
            }
        }

        var prompt = args != null ? string.Join(" ", args).Trim() : string.Empty;
        if (prompt.Length == 0)
        {
            return "Usage: ai <question>  (also: ai clear, ai compact)";
        }

        _ = presenter.AskAsync(prompt);
        return "Question queued";
    }
}
