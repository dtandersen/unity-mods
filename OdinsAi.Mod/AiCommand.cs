using Jotunn.Entities;

namespace OdinsAi.Mod
{
    /// <summary>
    /// The <c>ai</c> command. Registered through Jotunn, so it works in the console
    /// (<c>ai &lt;question&gt;</c>) and in chat (<c>/ai &lt;question&gt;</c>). Chat
    /// strips the leading slash and routes it as a command, so it is never broadcast
    /// to other players; output goes to the calling terminal only.
    /// </summary>
    public sealed class AiCommand : ConsoleCommand
    {
        public override string Name => "ai";

        public override string Help => "Ask Odin's Eye. Usage: /ai <question>  (also: /ai clear, /ai compact)";

        public override void Run(string[] args, Terminal context)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "clear":
                    case "reset":
                        _ = BepInExPlugin.context.ClearContextAsync(context);
                        return;

                    case "compact":
                        _ = BepInExPlugin.context.CompactContextAsync(context);
                        return;
                }
            }

            var prompt = string.Join(" ", args).Trim();
            if (prompt.Length == 0)
            {
                context.AddString("Usage: /ai <question>  (also: /ai clear, /ai compact)");
                return;
            }

            _ = BepInExPlugin.context.AskAsync(context, prompt);
        }
    }
}