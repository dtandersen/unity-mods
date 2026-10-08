namespace StationeersAi.Client.Model;

/// <summary>The value comes from a matched path template part.</summary>
public sealed class FromRouteAttribute : BindAttribute
{
    public FromRouteAttribute(string? name = null) : base(name)
    {
    }
}
