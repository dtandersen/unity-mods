using System;

namespace Sage.Client.Model;

/// <summary>The value names a thing by id: a refusal says "must be a thing id."</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ThingIdAttribute : Attribute
{
}
