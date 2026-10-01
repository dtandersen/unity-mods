using Util.Commands;

namespace StationeersAi.Mod;

/// <summary>
/// The <c>ai</c> console command: <c>ai &lt;question&gt;</c> asks the assistant,
/// <c>ai clear</c> drops the conversation, <c>ai compact</c> summarises it.
/// Questions are answered asynchronously; the answer is printed to the console
/// when the model replies.
/// </summary>
public sealed class AiConsoleCommand : CommandBase
{
    public override CommandScope Scope => CommandScope.InGame;

    public override string HelpText =>
        $"Ask {BepInExPlugin.AssistantName}. Usage: ai <question>  (also: ai clear, ai compact)";

    public override string[] Arguments => new[] { "<question> | clear | compact" };

    public override bool IsLaunchCmd => false;

    public override string Execute(string[] args)
    {
        if (args != null && args.Length > 0)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "window":
                    BepInExPlugin.context.ShowWindow();
                    return "Sage window opened.";

                case "clear":
                case "reset":
                    BepInExPlugin.context.ClearContext();
                    return "Context cleared.";

                case "compact":
                    BepInExPlugin.context.CompactContext();
                    return "Compacting context...";
            }
        }

        var prompt = args != null ? string.Join(" ", args).Trim() : string.Empty;
        if (prompt.Length == 0)
        {
            return $"Usage: ai <question>  (also: ai clear, ai compact)";
        }

        BepInExPlugin.context.Ask(prompt);
        return $"Asking {BepInExPlugin.AssistantName}... (the answer follows in the Sage window)";
    }
}
