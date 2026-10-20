using System.Text;

namespace Sage.Formatting;

/// <summary>
/// Short stable hash of IC source, so Sage can tell whether the editor
/// text matches the code burned into a housing without either side
/// printing the full text. FNV-1a 32-bit over UTF-8, as 8 hex digits.
/// </summary>
public static class CodeHash
{
    public static string Hash(string? code)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var byt in Encoding.UTF8.GetBytes(code ?? string.Empty))
            {
                hash ^= byt;
                hash *= 16777619;
            }

            return hash.ToString("x8");
        }
    }
}
