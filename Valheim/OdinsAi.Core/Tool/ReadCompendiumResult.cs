using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>Typed payload of the <c>read_compendium</c> tool.</summary>
    public sealed record ReadCompendiumResult(string Topic, string Text);
}