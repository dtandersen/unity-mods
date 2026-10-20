using System.Text.RegularExpressions;

namespace Sage.Formatting;

/// <summary>
/// Strips Unity rich-text tags (<c>color</c>, <c>size</c>, <c>link</c>, ...).
/// Game strings (display names, quantity text, Stationpedia pages) carry
/// these for the in-game UI; outside tools get plain text instead. Inner
/// text stays; a null input stays null.
/// </summary>
public static class UnityRichText
{
    private static readonly Regex Tag = new Regex(@"<[^<>]*>", RegexOptions.Compiled);

    public static string? Strip(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return Tag.Replace(text, string.Empty);
    }
}
