using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The loaded world, or why the mod cannot say.</summary>
public sealed record WorldResult(Status Status, List<string> Errors, WorldInfo? World) : BaseResult(Status, Errors);

/// <summary>Domain operation: read the loaded world.</summary>
public sealed class GetWorldCommand : Command<NoRequest, WorldResult>
{
    private readonly IWorldContext _world;

    public GetWorldCommand(IWorldContext world)
    {
        _world = world;
    }

    public WorldResult Execute(NoRequest request)
    {
        var info = _world.Info;
        return info == null
            ? new WorldResult(Status.NOT_FOUND, new List<string> { "World info is unavailable." }, null)
            : new WorldResult(Status.SUCCESS, new List<string>(), info);
    }
}
