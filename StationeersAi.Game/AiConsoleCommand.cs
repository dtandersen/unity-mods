using System;
using StationeersAi.Window;
using Util.Commands;

namespace StationeersAi.Game;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai clear</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer streams into the Sage
/// window when the model replies. The mod is resolved per call because the
/// shell rebuilds it every level.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    private readonly Func<SageMod> _mod;

    public AiConsoleCommand(Func<SageMod> mod)
    {
        _mod = mod ?? throw new ArgumentNullException(nameof(mod));
    }

    public override CommandScope Scope => CommandScope.InGame;

    public override string HelpText =>
        $"Ask {AssistantName()}. Usage: ai <question>  (also: ai clear, ai compact)";

    public override string[] Arguments => new[] { "<question> | clear | compact" };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        var presenter = _mod()?.ConsolePresenter();
        if (presenter == null)
        {
            return "Sage is not loaded. Load a world first.";
        }

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
            return $"Usage: ai <question>  (also: ai clear, ai compact)";
        }

        _ = presenter.AskAsync(prompt);
        return $"Asking {AssistantName()}... (the answer follows in the Sage window)";
    }

    private string AssistantName()
    {
        return _mod()?.AssistantName ?? "Sage";
    }
}
