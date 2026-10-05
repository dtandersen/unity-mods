using System;
using System.Collections.Generic;
using System.Reflection;

namespace StationeersAi.Server;

/// <summary>
/// Unity-free HTTP router for Sage. Finds the <see cref="SageRouteAttribute"/>
/// methods on <see cref="SageController"/> once, then calls the one matching
/// each <c>METHOD /sage/...</c> pair. Routes may hold <c>{name}</c> template
/// parts, which match any one path part; plain routes win over templates. A
/// parameter named <c>body</c> takes the request body; every other parameter
/// takes the path part or query value of the same name, with the path winning.
/// The mod hosts this over raw TCP; tests call <see cref="Handle"/> straight
/// with fakes.
/// </summary>
public sealed class SageRouter
{
    private static readonly IReadOnlyDictionary<string, MethodInfo> LiteralRoutes = FindLiteralRoutes();

    private static readonly IReadOnlyList<TemplateRoute> TemplateRoutes = FindTemplateRoutes();

    private readonly SageController _controller;

    public SageRouter(SageCommands commands)
    {
        _controller = new SageController(commands);
    }

    public SageResponse Handle(string method, string path, IDictionary<string, string> query, string? body = null)
    {
        if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(path))
        {
            return SageResponse.Error(400, "Expected a method and path.");
        }

        if (!method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            !method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            !method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            return SageResponse.Error(405, "Only GET, POST, and PUT serve here.");
        }

        var route = path.TrimEnd('/');
        if (route.StartsWith("/sage", StringComparison.OrdinalIgnoreCase))
        {
            route = route.Substring("/sage".Length);
            if (route.Length == 0)
            {
                route = "/";
            }
        }

        query = query ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        MethodInfo? target;
        IDictionary<string, string>? captured = null;
        if (!LiteralRoutes.TryGetValue(method.ToUpperInvariant() + " " + route.ToLowerInvariant(), out target) ||
            target is null)
        {
            target = MatchTemplate(method, route, out captured);
            if (target is null)
            {
                return SageResponse.Error(404, "No route serves " + route + ".");
            }
        }

        return (SageResponse)target.Invoke(_controller, Args(target, query, captured, body))!;
    }

    private static MethodInfo? MatchTemplate(string method, string route, out IDictionary<string, string>? captured)
    {
        captured = null;
        var parts = route.Trim('/').Split('/');
        foreach (var template in TemplateRoutes)
        {
            if (!template.Method.Equals(method, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (template.Parts.Length != parts.Length)
            {
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matches = true;
            for (var i = 0; i < parts.Length; i++)
            {
                var literal = template.Parts[i] as string;
                var par = template.Parts[i] as ParamPart;
                if (literal != null)
                {
                    if (!literal.Equals(parts[i], StringComparison.OrdinalIgnoreCase))
                    {
                        matches = false;
                        break;
                    }
                }
                else if (par != null)
                {
                    values[par.Name] = Unescape(parts[i]);
                }
            }

            if (matches)
            {
                captured = values;
                return template.Target;
            }
        }

        return null;
    }

    private static object?[] Args(MethodInfo target, IDictionary<string, string> query, IDictionary<string, string>? captured, string? body)
    {
        var pars = target.GetParameters();
        var args = new object?[pars.Length];
        for (var i = 0; i < pars.Length; i++)
        {
            var name = pars[i].Name ?? string.Empty;
            if (name.Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                args[i] = body;
                continue;
            }

            string? value = null;
            if (captured != null && captured.TryGetValue(name, out var fromPath))
            {
                value = fromPath;
            }
            else
            {
                value = Q(query, name);
            }

            args[i] = value;
        }

        return args;
    }

    private static IReadOnlyDictionary<string, MethodInfo> FindLiteralRoutes()
    {
        var routes = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
        foreach (var method in typeof(SageController).GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (SageRouteAttribute route in method.GetCustomAttributes(typeof(SageRouteAttribute), false))
            {
                if (!route.Route.Contains("{"))
                {
                    routes[route.Method.ToUpperInvariant() + " " + route.Route.ToLowerInvariant()] = method;
                }
            }
        }

        return routes;
    }

    private static IReadOnlyList<TemplateRoute> FindTemplateRoutes()
    {
        var routes = new List<TemplateRoute>();
        foreach (var method in typeof(SageController).GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (SageRouteAttribute route in method.GetCustomAttributes(typeof(SageRouteAttribute), false))
            {
                if (route.Route.Contains("{"))
                {
                    routes.Add(new TemplateRoute(route.Method, Split(route.Route), method));
                }
            }
        }

        return routes;
    }

    private static object[] Split(string route)
    {
        var parts = route.Trim('/').Split('/');
        var parsed = new object[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            ParamPart par;
            parsed[i] = ParamPart.TryParse(parts[i], out par) ? (object)par : (object)parts[i];
        }

        return parsed;
    }

    private static string? Q(IDictionary<string, string> query, string name)
    {
        string? value;
        if (query.TryGetValue(name, out value))
        {
            return value;
        }

        return null;
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

    private sealed class TemplateRoute
    {
        public TemplateRoute(string method, object[] parts, MethodInfo target)
        {
            Method = method;
            Parts = parts;
            Target = target;
        }

        public string Method { get; }

        public object[] Parts { get; }

        public MethodInfo Target { get; }
    }

    private sealed class ParamPart
    {
        private ParamPart(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static bool TryParse(string part, out ParamPart par)
        {
            par = null!;
            if (part.Length < 3 || !part.StartsWith("{", StringComparison.Ordinal) || !part.EndsWith("}", StringComparison.Ordinal))
            {
                return false;
            }

            var name = part.Substring(1, part.Length - 2).Trim();
            if (name.Length == 0)
            {
                return false;
            }

            par = new ParamPart(name);
            return true;
        }
    }
}
