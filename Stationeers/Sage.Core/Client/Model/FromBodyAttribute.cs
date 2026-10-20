namespace Sage.Client.Model;

/// <summary>The value comes from the JSON body.</summary>
public sealed class FromBodyAttribute : BindAttribute
{
    public FromBodyAttribute(string? name = null) : base(name)
    {
    }
}
