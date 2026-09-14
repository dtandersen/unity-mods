using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <c>`code`</c>, <c>~~strike~~</c>, links, headings, bullet lists, and tables.
    /// Tables are padded into fixed-width columns and wrapped in TMP's
    /// <c>&lt;mspace&gt;</c> tag so the columns line up despite the proportional font.
    /// </remarks>
    public static class MarkdownToTmp
    {
        private const string CodeColor = "#E0C46C";

        // TMP's <mspace> tag forces a fixed advance per character. 0.7em leaves
        // enough room for wide glyphs that they don't collide with the next cell.
        private const string MonoSpace = "0.7em";
        private const string ColumnSeparator = "  ";

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

        private static readonly Regex TableSeparatorCell =
            new Regex(@"^:?-+:?$", RegexOptions.Compiled);

        private enum Alignment
        {
            Left,
            Center,
            Right,
        }

        public static string ToTmpRichText(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return markdown ?? string.Empty;
            }

            var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var output = new List<string>(lines.Length);

            for (var i = 0; i < lines.Length; i++)
            {
                var raw = lines[i];
                if (CodeFence.IsMatch(raw))
                {
                    continue;
                }

                if (IsTableRow(raw) && i + 1 < lines.Length && IsSeparatorRow(lines[i + 1]))
                {
                    var block = new List<string> { raw, lines[i + 1] };
                    var next = i + 2;
                    while (next < lines.Length && IsTableRow(lines[next]))
                    {
                        block.Add(lines[next]);
                        next++;
                    }

                    output.Add(RenderTable(block));
                    i = next - 1;
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

        /// <summary>Renders a table block (header row, separator row, body rows).</summary>
        private static string RenderTable(IReadOnlyList<string> rows)
        {
            var parsed = rows.Select(SplitCells).ToArray();
            var columns = parsed.Max(cells => cells.Length);
            var alignments = ParseAlignments(parsed[1], columns);
            var rendered = parsed.Select(cells => cells.Select(Inline).ToArray()).ToArray();

            var widths = new int[columns];
            for (var c = 0; c < columns; c++)
            {
                for (var r = 0; r < rendered.Length; r++)
                {
                    // Row 1 is the alignment separator, not content.
                    if (r == 1)
                    {
                        continue;
                    }

                    widths[c] = Math.Max(widths[c], VisibleLength(Cell(rendered[r], c)));
                }
            }

            var lines = new List<string>
            {
                ContentRow(rendered[0], widths, alignments),
                string.Join(ColumnSeparator, widths.Select(width => new string('-', width))),
            };

            for (var r = 2; r < rendered.Length; r++)
            {
                lines.Add(ContentRow(rendered[r], widths, alignments));
            }

            return $"<mspace={MonoSpace}>" + string.Join("\n", lines) + "</mspace>";
        }

        private static string ContentRow(string[] cells, int[] widths, Alignment[] alignments)
        {
            var parts = new string[widths.Length];
            for (var c = 0; c < widths.Length; c++)
            {
                var text = Cell(cells, c);
                parts[c] = Pad(text, VisibleLength(text), widths[c], alignments[c]);
            }

            // Nothing follows the last column, so drop its trailing padding.
            return string.Join(ColumnSeparator, parts).TrimEnd();
        }

        private static string Pad(string text, int length, int width, Alignment alignment)
        {
            var padding = width - length;
            if (padding <= 0)
            {
                return text;
            }

            switch (alignment)
            {
                case Alignment.Right:
                    return new string(' ', padding) + text;
                case Alignment.Center:
                    var left = padding / 2;
                    return new string(' ', left) + text + new string(' ', padding - left);
                default:
                    return text + new string(' ', padding);
            }
        }

        private static Alignment[] ParseAlignments(string[] separator, int columns)
        {
            var alignments = new Alignment[columns];
            for (var c = 0; c < columns; c++)
            {
                var cell = Cell(separator, c);
                var left = cell.StartsWith(":");
                var right = cell.EndsWith(":");
                alignments[c] = left && right ? Alignment.Center : right ? Alignment.Right : Alignment.Left;
            }

            return alignments;
        }

        /// <summary>Splits a table row into trimmed cells, honouring <c>\|</c> escapes.</summary>
        private static string[] SplitCells(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("|"))
            {
                trimmed = trimmed.Substring(1);
            }

            if (trimmed.EndsWith("|"))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            var cells = new List<string>();
            var current = new StringBuilder();
            for (var i = 0; i < trimmed.Length; i++)
            {
                var c = trimmed[i];
                if (c == '\\' && i + 1 < trimmed.Length && trimmed[i + 1] == '|')
                {
                    current.Append('|');
                    i++;
                    continue;
                }

                if (c == '|')
                {
                    cells.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            cells.Add(current.ToString().Trim());
            return cells.ToArray();
        }

        private static bool IsTableRow(string line)
        {
            return line.IndexOf('|') >= 0;
        }

        private static bool IsSeparatorRow(string line)
        {
            if (!IsTableRow(line))
            {
                return false;
            }

            return SplitCells(line).All(cell => TableSeparatorCell.IsMatch(cell));
        }

        private static string Cell(string[] cells, int index)
        {
            return index < cells.Length ? cells[index] : string.Empty;
        }

        /// <summary>Length of the text with TMP tags removed, in characters.</summary>
        private static int VisibleLength(string value)
        {
            var length = 0;
            var inTag = false;
            foreach (var c in value)
            {
                if (c == '<')
                {
                    inTag = true;
                }
                else if (c == '>')
                {
                    inTag = false;
                }
                else if (!inTag)
                {
                    length++;
                }
            }

            return length;
        }
    }
}