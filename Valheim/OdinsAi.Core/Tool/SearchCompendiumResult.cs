using System.Collections.Generic;

namespace OdinsAi.Tool
{
    /// <summary>A compendium page's topic with a short preview of its text.</summary>
    public sealed record CompendiumSummary(string Topic, string Excerpt);

    /// <summary>Typed payload of the <c>search_compendium</c> tool.</summary>
    public sealed record SearchCompendiumResult(IReadOnlyList<CompendiumSummary> Entries);
}