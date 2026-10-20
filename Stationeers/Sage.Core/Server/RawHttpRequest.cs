using System;
using System.Collections.Generic;

namespace Sage.Server;

/// <summary>
/// One parsed HTTP request: method, path, query, headers, and body text.
/// Pure parsing with no sockets, so tests cover it without the game.
/// </summary>
public sealed class RawHttpRequest
{
    public string Method { get; set; } = null!;

    public string Path { get; set; } = null!;

    public IDictionary<string, string> Query { get; set; } = null!;

    public IDictionary<string, string> Headers { get; set; } = null!;

    public string? Body { get; set; }

    /// <summary>
    /// Parses the head (<c>METHOD target HTTP/x</c> plus <c>Name: value</c>
    /// lines) and pairs it with an already-read body. Returns null when the
    /// head is not a request we serve.
    /// </summary>
    public static RawHttpRequest? TryParse(string head, string? body)
    {
        if (string.IsNullOrWhiteSpace(head))
        {
            return null;
        }

        var lines = head.Split(new[] { "\r\n" }, StringSplitOptions.None);
        if (lines.Length == 0)
        {
            return null;
        }

        var first = lines[0].Split(' ');
        if (first.Length < 2)
        {
            return null;
        }

        var method = first[0].Trim().ToUpperInvariant();
        if (method != "GET" && method != "POST" && method != "PUT")
        {
            return null;
        }

        var target = first[1].Trim();
        // Proxies send absolute-form; strip any scheme and host.
        if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var slash = target.IndexOf('/', target.IndexOf("://", StringComparison.Ordinal) + 3);
            target = slash >= 0 ? target.Substring(slash) : "/";
        }

        string path = target;
        var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var q = target.IndexOf('?');
        if (q >= 0)
        {
            path = target.Substring(0, q);
            foreach (var pair in target.Substring(q + 1).Split('&'))
            {
                if (string.IsNullOrWhiteSpace(pair))
                {
                    continue;
                }

                var eq = pair.IndexOf('=');
                if (eq < 0)
                {
                    query[Unescape(pair)] = string.Empty;
                }
                else
                {
                    query[Unescape(pair.Substring(0, eq))] = Unescape(pair.Substring(eq + 1));
                }
            }
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < lines.Length; i++)
        {
            var colon = lines[i].IndexOf(':');
            if (colon > 0)
            {
                headers[lines[i].Substring(0, colon).Trim()] = lines[i].Substring(colon + 1).Trim();
            }
        }

        return new RawHttpRequest
        {
            Method = method,
            Path = path,
            Query = query,
            Headers = headers,
            Body = body,
        };
    }

    private static string Unescape(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value.Replace('+', ' '));
        }
        catch (Exception)
        {
            return value;
        }
    }
}
