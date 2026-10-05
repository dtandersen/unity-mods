using System;

namespace StationeersAi.Server;

/// <summary>
/// Marks a <see cref="SageController"/> method as serving one HTTP route.
/// The router finds these by reflection and calls the match, so adding a
/// route means adding one annotated method. Use <see cref="SageGet"/> for
/// reads, <see cref="SagePost"/> for writes, and <see cref="SagePut"/> for
/// updates.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class SageRouteAttribute : Attribute
{
    protected SageRouteAttribute(string method, string route, string? summary = null)
    {
        Method = method;
        Route = route;
        Summary = summary;
    }

    public string Method { get; }

    public string Route { get; }

    public string? Summary { get; }
}

/// <summary>Serves <c>GET</c> on the given <c>/sage/...</c> path.</summary>
public sealed class SageGetAttribute : SageRouteAttribute
{
    public SageGetAttribute(string route, string? summary = null)
        : base("GET", route, summary)
    {
    }
}

/// <summary>Serves <c>POST</c> on the given <c>/sage/...</c> path.</summary>
public sealed class SagePostAttribute : SageRouteAttribute
{
    public SagePostAttribute(string route, string? summary = null)
        : base("POST", route, summary)
    {
    }
}

/// <summary>Serves <c>PUT</c> on the given <c>/sage/...</c> path.</summary>
public sealed class SagePutAttribute : SageRouteAttribute
{
    public SagePutAttribute(string route, string? summary = null)
        : base("PUT", route, summary)
    {
    }
}
