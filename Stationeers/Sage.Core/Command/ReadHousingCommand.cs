using System.Collections.Generic;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Command;

/// <summary>Ask for the program burned on one housing's chip.</summary>
public sealed record ReadHousingRequest(long Device);

/// <summary>The burned program, or why the read did not happen.</summary>
public sealed record ProgramResult(Status Status, List<string> Errors, Program? Program) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: read the program burned on one housing's chip. A
/// missed id becomes <see cref="Status.NOT_FOUND"/> with the full sentence;
/// an unreadable chip escapes toward the router, which answers 500.
/// </summary>
public sealed class ReadHousingCommand : Command<ReadHousingRequest, ProgramResult>
{
    private readonly IIcDevices _devices;

    public ReadHousingCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ProgramResult Execute(ReadHousingRequest request)
    {
        try
        {
            return new ProgramResult(
                Status.SUCCESS, new List<string>(), _devices.FindHousing(request.Device).Read());
        }
        catch (ThingNotFound notFound)
        {
            return new ProgramResult(Status.NOT_FOUND, new List<string> { notFound.Message }, null);
        }
    }
}
