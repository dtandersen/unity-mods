using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StationeersAi.Formatting;

/// <summary>
/// Strips Markdown down to plain text. The Stationeers console is plain text,
/// so this keeps an answer readable instead of leaking formatting markers.
/// </summary>
public static class MarkdownToPlain
{
    private static readonly Regex Heading =
        new Regex(@"^\s{0,3}#{1,6}\s+(.*?)\s*#*\s*$", RegexOptions.Compiled);

    private static readonly Regex Bullet =
        new Regex(@"^(\s*)[-*+]\s+(.*)$", RegexOptions.Compiled);

    private static readonly Regex CodeFence =
        new Regex(@"^\s*```.*$", RegexOptions.Compiled);

    private static readonly Regex Code =
        new Regex(@"`(.+?)`", RegexOptions.Compiled);

    private static readonly Regex Bold =
        new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled);

    private static readonly Regex BoldAlt =
        new Regex(@"__(.+?)__", RegexOptions.Compiled);

    private static readonly Regex Strike =
        new Regex(@"~~(.+?)~~", RegexOptions.Compiled);

    private static readonly Regex Italic =
        new Regex(@"(?<!\*)\*(?!\s)(.+?)(?<!\s)\*(?!\*)", RegexOptions.Compiled);

    private static readonly Regex ItalicAlt =
        new Regex(@"(?<![\w_])_(?!\s)(.+?)(?<!\s)_(?![\w_])", RegexOptions.Compiled);

    private static readonly Regex Link =
        new Regex(@"\[(.+?)\]\((.+?)\)", RegexOptions.Compiled);

    public static string ToPlainText(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown ?? string.Empty;
        }

        var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var output = new List<string>(lines.Length);

        foreach (var raw in lines)
        {
            if (CodeFence.IsMatch(raw))
            {
                continue;
            }

            var line = raw.TrimEnd();

            var heading = Heading.Match(line);
            if (heading.Success)
            {
                line = heading.Groups[1].Value;
            }
            else
            {
                var bullet = Bullet.Match(line);
                if (bullet.Success)
                {
                    line = $"{bullet.Groups[1].Value}• {bullet.Groups[2].Value}";
                }
            }

            output.Add(Inline(line));
        }

        return string.Join("\n", output).Trim();
    }

    private static string Inline(string line)
    {
        line = Code.Replace(line, "$1");
        line = Bold.Replace(line, "$1");
        line = BoldAlt.Replace(line, "$1");
        line = Strike.Replace(line, "$1");
        line = Italic.Replace(line, "$1");
        line = ItalicAlt.Replace(line, "$1");
        line = Link.Replace(line, "$1");
        return line;
    }
}
