using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: find loose world things near a point.</summary>
public sealed class SearchThingsCommand
{
    private readonly IThingCatalog _things;
    private readonly IPlayerContext _player;

    public SearchThingsCommand(IThingCatalog things, IPlayerContext player)
    {
        _things = things;
        _player = player;
    }

    /// <returns>Matches, the origin used (the player's position when omitted), and the player's facing.</returns>
    public (IReadOnlyList<ThingInfo> Things, OriginPoint Origin, double? Facing) Search(
        string query,
        OriginPoint origin,
        double? maxDistanceM)
    {
        var status = _player.Status;
        var resolved = origin ?? (status == null
            ? null
            : new OriginPoint(status.X, status.Y, status.Z));
        return (_things.Search(query, resolved, maxDistanceM), resolved, status?.FacingYaw);
    }
}
