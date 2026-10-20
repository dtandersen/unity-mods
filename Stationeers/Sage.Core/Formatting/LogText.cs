using System.Text;

namespace Sage.Formatting;

/// <summary>
/// Keeps log lines readable where they land: the game log file mangles
/// anything outside Latin-1 (model emoji arrives as U+FFFD). Common
/// punctuation folds to ASCII; anything else becomes '?'.
/// </summary>
public static class LogText
{
    public static string Sanitize(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        var clean = new StringBuilder(message.Length);
        foreach (var character in message)
        {
            clean.Append(character <= 0xFF ? character.ToString() : Fold(character));
        }

        return clean.ToString();
    }

    private static string Fold(char character)
    {
        switch (character)
        {
            case '\u2014':
            case '\u2013':
            case '\u2022':
                return "-";
            case '\u2026':
                return "...";
            default:
                return "?";
        }
    }
}
