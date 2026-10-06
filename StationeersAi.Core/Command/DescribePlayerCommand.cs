using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: describe the player's current status.</summary>
public sealed class DescribePlayerCommand
{
    private readonly IPlayerContext _player;

    public DescribePlayerCommand(IPlayerContext player)
    {
        _player = player;
    }

    public PlayerStatus? Execute()
    {
        return _player.Status;
    }
}
