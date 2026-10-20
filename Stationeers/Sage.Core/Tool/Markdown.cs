using System.Collections.Generic;
using System.Linq;

namespace Sage.Tool;

/// <summary>Minimal Markdown table writer for tool output handed to the model.</summary>
internal static class Markdown
{
    public static string Build(string[] header, IEnumerable<string?[]> rows)
    {
        var body = rows.Select(row => row.ToArray()).ToArray();
        var widths = header
            .Select((name, index) => body
                .Select(row => index < row.Length ? row[index] ?? string.Empty : string.Empty)
                .Concat(new[] { name })
                .Max(cell => cell.Length))
            .ToArray();

        var lines = new List<string> { Header(header, widths) };
        lines.Add("| " + string.Join(" | ", widths.Select(width => new string('-', width))) + " |");
        foreach (var row in body)
        {
            lines.Add(Row(row, widths));
        }

        return string.Join("\n", lines);
    }

    private static string Header(string?[] cells, int[] widths)
    {
        var padded = widths.Select((width, index) =>
            (index < cells.Length ? cells[index] ?? string.Empty : string.Empty).PadRight(width));
        return "| " + string.Join(" | ", padded) + " |";
    }

    private static string Row(string?[] cells, int[] widths)
    {
        var padded = widths.Select((width, index) =>
        {
            var cell = index < cells.Length ? cells[index] ?? string.Empty : string.Empty;
            return double.TryParse(cell, out _) ? cell.PadLeft(width) : cell.PadRight(width);
        });
        return "| " + string.Join(" | ", padded) + " |";
    }
}
