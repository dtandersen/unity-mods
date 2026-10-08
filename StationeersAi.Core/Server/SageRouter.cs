using StationeersAi.Client.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StationeersAi.Core;

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

    private readonly Logger _log;

    public SageRouter(SageCommands commands, Logger log)
    {
        _controller = new SageController(commands);
        _log = log;
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

        try
        {
            return (SageResponse)target.Invoke(_controller, Args(target, query, captured, body))!;
        }
        catch (RequestRejected rejected)
        {
            // The router could not shape the ask into the model: name what
            // failed to convert and answer 400.
            return SageResponse.Error(400, rejected.Message);
        }
        catch (TargetInvocationException ex)
        {
            // Commands answer with results. Anything still escaping a
            // controller call is a fault: log it, name it once, ship 500.
            var inner = ex.InnerException ?? ex;
            _log.Error(inner);
            return SageResponse.Error(500, "Command failed.");
        }
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
            var type = pars[i].ParameterType;
            if (type != typeof(string))
            {
                args[i] = BuildModel(type, captured, query, body);
                continue;
            }

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

    /// <summary>Where the router reads a request-model property from.</summary>
    private enum BindSource
    {
        Route,
        Query,
        Body,
    }

    /// <summary>One property of a request model: its key, its kind, and its home.</summary>
    private sealed class PropertyBinding
    {
        public string Key = string.Empty;
        public PropertyInfo Property = null!;
        public Type Underlying = null!;
        public BindSource Source;
        public bool Optional;
        public bool Thing;
    }

    private static readonly Dictionary<Type, PropertyBinding[]> ModelBindings = new Dictionary<Type, PropertyBinding[]>();

    private static readonly object ModelBindingsLock = new object();

    /// <summary>
    /// Fills a request model from the matched path, the query, and the JSON
    /// body. A value that will not convert to the declared type raises
    /// <see cref="RequestRejected"/>, and the router answers 400 with its
    /// name: a thing id names itself, the rest say they are no number.
    /// </summary>
    private static object BuildModel(Type modelType, IDictionary<string, string>? captured, IDictionary<string, string> query, string? body)
    {
        var model = Activator.CreateInstance(modelType)
            ?? throw new InvalidOperationException("A request model needs a public default constructor.");

        PropertyBinding[] bindings;
        lock (ModelBindingsLock)
        {
            if (!ModelBindings.TryGetValue(modelType, out bindings!))
            {
                bindings = ReadBindings(modelType);
                ModelBindings[modelType] = bindings;
            }
        }

        JObject? json = null;
        foreach (var binding in bindings)
        {
            string? value;
            if (binding.Source == BindSource.Route)
            {
                string? fromPath;
                value = captured != null && captured.TryGetValue(binding.Key, out fromPath) ? fromPath : null;
            }
            else if (binding.Source == BindSource.Query)
            {
                value = Q(query, binding.Key);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    continue;
                }

                if (json == null)
                {
                    json = ParseJson(body);
                }

                var token = json.GetValue(binding.Key, StringComparison.OrdinalIgnoreCase);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
                }

                value = token.ToString();
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!binding.Optional && binding.Source == BindSource.Route)
                {
                    throw new RequestRejected(Refusal(binding));
                }

                continue;
            }

            binding.Property.SetValue(model, Convert(value, binding));
        }

        return model;
    }

    private static JObject ParseJson(string body)
    {
        try
        {
            return JObject.Parse(body);
        }
        catch (JsonException)
        {
            throw new RequestRejected("body is not valid json.");
        }
    }

    private static object Convert(string value, PropertyBinding binding)
    {
        if (binding.Underlying == typeof(string))
        {
            return value;
        }

        if (binding.Underlying == typeof(long))
        {
            long whole;
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out whole))
            {
                return whole;
            }
        }
        else if (binding.Underlying == typeof(double))
        {
            double number;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return number;
            }
        }
        else if (binding.Underlying == typeof(bool))
        {
            bool flag;
            if (bool.TryParse(value, out flag))
            {
                return flag;
            }

            if (value == "1")
            {
                return true;
            }

            if (value == "0")
            {
                return false;
            }
        }
        else
        {
            throw new InvalidOperationException("A request model property must be string, long, double, or bool: " + binding.Underlying.Name);
        }

        throw new RequestRejected(Refusal(binding));
    }

    private static string Refusal(PropertyBinding binding)
    {
        if (binding.Thing)
        {
            return binding.Key + " must be a thing id.";
        }

        if (binding.Underlying == typeof(bool))
        {
            return binding.Key + " must be true or false.";
        }

        return binding.Key + " must be a number.";
    }

    private static PropertyBinding[] ReadBindings(Type modelType)
    {
        var bindings = new List<PropertyBinding>();
        foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var bind = (BindAttribute?)Attribute.GetCustomAttribute(property, typeof(BindAttribute));
            if (bind == null)
            {
                continue;
            }

            var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            bindings.Add(new PropertyBinding
            {
                Key = bind.Name ?? CamelName(property.Name),
                Property = property,
                Underlying = underlying,
                Source = bind is FromRouteAttribute ? BindSource.Route : bind is FromQueryAttribute ? BindSource.Query : BindSource.Body,
                Optional = underlying == typeof(string) || Nullable.GetUnderlyingType(property.PropertyType) != null,
                Thing = Attribute.IsDefined(property, typeof(ThingIdAttribute)),
            });
        }

        return bindings.ToArray();
    }

    private static string CamelName(string name)
    {
        if (name.Length == 0)
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>A value the caller sent that the route cannot read.</summary>
    private sealed class RequestRejected : Exception
    {
        public RequestRejected(string message) : base(message)
        {
        }
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
