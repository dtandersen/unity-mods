using System.Collections.Generic;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Command;

/// <summary>What the player looks like right now, or why the mod cannot say.</summary>
public sealed record PlayerResult(Status Status, List<string> Errors, Player? Player) : BaseResult(Status, Errors);

/// <summary>Domain operation: read the player's status.</summary>
public sealed class DescribePlayerCommand : Command<NoRequest, PlayerResult>
{
    private readonly IPlayerContext _player;

    public DescribePlayerCommand(IPlayerContext player)
    {
        _player = player;
    }

    public PlayerResult Execute(NoRequest request)
    {
        var status = _player.Status;
        return status == null
            ? new PlayerResult(Status.NOT_FOUND, new List<string> { "Player status is unavailable." }, null)
            : new PlayerResult(Status.SUCCESS, new List<string>(), status);
    }
}
