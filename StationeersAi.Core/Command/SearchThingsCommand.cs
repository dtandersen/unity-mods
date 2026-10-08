using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Ask for loose things near an origin; a null origin means the player's position.</summary>
public sealed record SearchThingsRequest(string? Query, Point? Origin, double? MaxDistanceM);

/// <summary>
/// The matches, the origin used, and the player's facing. An empty match
/// list is still success: nothing nearby is an answer, not a fault.
/// </summary>
public sealed record ThingSearchResult(
    Status Status,
    List<string> Errors,
    IReadOnlyList<ThingInfo> Things,
    Point? Origin,
    double? Facing) : BaseResult(Status, Errors);

/// <summary>Domain operation: search loose world things by name and distance.</summary>
public sealed class SearchThingsCommand : Command<SearchThingsRequest, ThingSearchResult>
{
    private readonly IThingCatalog _things;
    private readonly IPlayerContext _player;

    public SearchThingsCommand(IThingCatalog things, IPlayerContext player)
    {
        _things = things;
        _player = player;
    }

    public ThingSearchResult Execute(SearchThingsRequest request)
    {
        var status = _player.Status;
        var resolved = request.Origin ?? (status == null
            ? null
            : new Point(status.X, status.Y, status.Z));
        return new ThingSearchResult(
            Status.SUCCESS,
            new List<string>(),
            _things.Search(request.Query, resolved, request.MaxDistanceM),
            resolved,
            status?.FacingYaw);
    }
}
