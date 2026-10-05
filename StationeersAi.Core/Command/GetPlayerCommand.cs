using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: report the player's current status.</summary>
public sealed class GetPlayerCommand
{
    private readonly IPlayerContext _player;

    public GetPlayerCommand(IPlayerContext player)
    {
        _player = player;
    }

    public PlayerStatus? Execute()
    {
        return _player.Status;
    }
}
