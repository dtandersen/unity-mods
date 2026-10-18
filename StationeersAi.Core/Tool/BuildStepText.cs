using System;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>One build step as the model reads it: "2. Wrench - Iron Sheets x2".</summary>
internal static class BuildStepText
{
    internal static string Format(BuildStep step)
    {
        var tools = (step.Entries ?? Array.Empty<BuildEntry>())
            .Where(entry => entry.Quantity == null)
            .Select(entry => entry.Name)
            .ToArray();
        var materials = (step.Entries ?? Array.Empty<BuildEntry>())
            .Where(entry => entry.Quantity != null)
            .Select(entry => $"{entry.Name} x{SearchRecipesTool.FormatAmount(entry.Quantity!.Value)}")
            .ToArray();
        var needs = tools.Length == 0
            ? string.Join("; ", materials)
            : materials.Length == 0
                ? string.Join(", ", tools)
                : $"{string.Join(", ", tools)} - {string.Join("; ", materials)}";

        return $"{step.State}. {needs}";
    }
}
