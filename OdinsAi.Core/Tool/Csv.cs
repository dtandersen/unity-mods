using System.Collections.Generic;
using System.Linq;

namespace OdinsAi.Tool
{
    /// <summary>Minimal CSV writer for tool output handed to the model.</summary>
    internal static class Csv
    {
        public static string Build(string[] header, IEnumerable<string[]> rows)
        {
            var lines = new List<string> { Join(header) };
            lines.AddRange(rows.Select(Join));
            return string.Join("\n", lines);
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join(",", values.Select(Escape));
        }

        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0
                ? value
                : "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}