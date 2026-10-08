using System;

namespace StationeersAi.Client.Model;

/// <summary>Where the router reads a request-model property from.</summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class BindAttribute : Attribute
{
    protected BindAttribute(string? name = null)
    {
        Name = name;
    }

    /// <summary>The key to look for, when it differs from the property's camel name.</summary>
    public string? Name { get; }
}
