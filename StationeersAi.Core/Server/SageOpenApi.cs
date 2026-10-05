using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using StationeersAi.Core;

namespace StationeersAi.Server;

/// <summary>
/// Builds a minimal OpenAPI 3.0.3 description of <see cref="SageController"/>
/// from its <see cref="SageRouteAttribute"/> marks. Lists each path, method,
/// query value, and body, so outside agents can find and call the routes.
/// Holds no game state.
/// </summary>
public static class SageOpenApi
{
    public static JObject Describe()
    {
        var paths = new JObject();
        foreach (var method in typeof(SageController).GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (SageRouteAttribute route in method.GetCustomAttributes(typeof(SageRouteAttribute), false))
            {
                var key = "/sage" + route.Route;
                var item = paths[key] as JObject;
                if (item is null)
                {
                    item = new JObject();
                    paths[key] = item;
                }

                item[route.Method.ToLowerInvariant()] = OperationBody(method, route);
            }
        }

        return new JObject
        {
            ["openapi"] = "3.0.3",
            ["info"] = new JObject
            {
                ["title"] = "Sage",
                ["version"] = CoreAssembly.Version,
            },
            ["paths"] = paths,
        };
    }

    private static JObject OperationBody(MethodInfo method, SageRouteAttribute route)
    {
        var pathNames = new HashSet<string>(Placeholders(route.Route), StringComparer.OrdinalIgnoreCase);
        var pars = new JArray();
        var takesBody = false;
        foreach (var par in method.GetParameters())
        {
            var name = par.Name ?? string.Empty;
            if (name.Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                takesBody = true;
            }
            else
            {
                var inPath = pathNames.Contains(name);
                pars.Add(new JObject
                {
                    ["name"] = name,
                    ["in"] = inPath ? "path" : "query",
                    ["required"] = inPath,
                    ["schema"] = new JObject { ["type"] = "string" },
                });
            }
        }

        var op = new JObject
        {
            ["operationId"] = OperationId(method.Name),
            ["responses"] = new JObject
            {
                ["200"] = new JObject { ["description"] = "OK" },
            },
        };

        if (!string.IsNullOrWhiteSpace(route.Summary))
        {
            op["summary"] = route.Summary;
        }

        if (pars.Count > 0)
        {
            op["parameters"] = pars;
        }

        if (takesBody)
        {
            op["requestBody"] = new JObject
            {
                ["required"] = false,
                ["content"] = new JObject
                {
                    ["application/json"] = new JObject
                    {
                        ["schema"] = new JObject { ["type"] = "object" },
                    },
                },
            };
        }

        return op;
    }

    private static string OperationId(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static IEnumerable<string> Placeholders(string route)
    {
        foreach (var part in route.Trim('/').Split('/'))
        {
            if (part.StartsWith("{", StringComparison.Ordinal) && part.EndsWith("}", StringComparison.Ordinal))
            {
                var name = part.Substring(1, part.Length - 2).Trim();
                if (name.Length > 0)
                {
                    yield return name;
                }
            }
        }
    }
}
