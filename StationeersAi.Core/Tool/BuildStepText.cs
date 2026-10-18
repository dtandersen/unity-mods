using System;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>One build step as the model reads it: "2. Wrench - Iron Sheets x2".</summary>
internal static class BuildStepText
{
    internal static string Format(BuildStep step)
    {
        var materials = string.Join("; ", (step.Materials ?? Array.Empty<Ingredient>())
            .Select(material => $"{material.ItemId} x{SearchRecipesTool.FormatAmount(material.Amount)}"));
        var needs = string.IsNullOrWhiteSpace(step.Tool)
            ? materials
            : string.IsNullOrWhiteSpace(materials) ? step.Tool : $"{step.Tool} - {materials}";

        return $"{step.State}. {needs}";
    }
}
