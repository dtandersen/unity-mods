namespace Sage.Client.Model;

/// <summary>The value comes from the query string.</summary>
public sealed class FromQueryAttribute : BindAttribute
{
    public FromQueryAttribute(string? name = null) : base(name)
    {
    }
}
