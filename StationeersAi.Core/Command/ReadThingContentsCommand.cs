using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: list what a container thing holds.</summary>
public sealed class ReadThingContentsCommand
{
    private readonly IThingContents _contents;

    public ReadThingContentsCommand(IThingContents contents)
    {
        _contents = contents;
    }

    public IReadOnlyList<ThingContent> Execute(string thing)
    {
        return _contents.Read(thing);
    }
}
