using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OdinsAi.Formatting
{
    /// <summary>
    /// Converts a small subset of Markdown into TextMeshPro rich-text tags. Valheim's
    /// chat/console (and HUD) render TMP tags, not Markdown, so model output is run
    /// through this before display.
    /// </summary>
    /// <remarks>
    /// Supported: <c>**bold**</c>, <c>__bold__</c>, <c>*italic*</c>, <c>_italic_</c>,
    /// <c>`code`</c>, <c>~~strike~~</c>, links, headings and bullet lists. Tables are
    /// intentionally not handled.
    /// </remarks>
    public static class MarkdownToTmp
    {
        private const string CodeColor = "#E0C46C";

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

        public static string ToTmpRichText(string markdown)
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
                    line = $"<b>{heading.Groups[1].Value}</b>";
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

            return string.Join("\n", output);
        }

        private static string Inline(string line)
        {
            line = Code.Replace(line, $"<color={CodeColor}>$1</color>");
            line = Bold.Replace(line, "<b>$1</b>");
            line = BoldAlt.Replace(line, "<b>$1</b>");
            line = Strike.Replace(line, "<s>$1</s>");
            line = Italic.Replace(line, "<i>$1</i>");
            line = ItalicAlt.Replace(line, "<i>$1</i>");
            line = Link.Replace(line, "$1");
            return line;
        }
    }
}