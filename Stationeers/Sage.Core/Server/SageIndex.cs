using System;
using System.Reflection;
using System.Text;

namespace Sage.Server;

/// <summary>
/// Builds a short Markdown listing of <see cref="SageController"/> from its
/// <see cref="SageRouteAttribute"/> marks: one line per route with its query
/// values. Shorter than the OpenAPI JSON for quick reads.
/// </summary>
public static class SageIndex
{
    public static string Describe()
    {
        var text = new StringBuilder();
        text.AppendLine("# Sage API");
        text.AppendLine();
        foreach (var method in typeof(SageController).GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (SageRouteAttribute route in method.GetCustomAttributes(typeof(SageRouteAttribute), false))
            {
                text.Append(route.Method.ToUpperInvariant());
                text.Append(" /sage");
                text.Append(route.Route);
                var query = Query(method, route.Route);
                if (query.Length > 0)
                {
                    text.Append("?");
                    text.Append(query);
                }

                if (TakesBody(method))
                {
                    text.Append(" (json body)");
                }

                if (!string.IsNullOrWhiteSpace(route.Summary))
                {
                    text.Append(" - ");
                    text.Append(route.Summary);
                }

                text.AppendLine();
            }
        }

        return text.ToString();
    }

    private static string Query(MethodInfo method, string route)
    {
        var names = new StringBuilder();
        foreach (var par in method.GetParameters())
        {
            var name = par.Name ?? string.Empty;
            if (name.Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (route.Contains("{" + name + "}"))
            {
                continue;
            }

            if (names.Length > 0)
            {
                names.Append("&");
            }

            names.Append(name);
        }

        return names.ToString();
    }

    private static bool TakesBody(MethodInfo method)
    {
        foreach (var par in method.GetParameters())
        {
            if ((par.Name ?? string.Empty).Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
