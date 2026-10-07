using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>
/// A computer's name, the thing id of the housing under its dropdown
/// (0 when none), and its screen text with the hash of that text.
/// Filled only when the status is <see cref="Status.SUCCESS"/>.
/// </summary>
public sealed record ReadComputerResult(
    Status Status,
    List<string> Errors,
    string? Name,
    long Device,
    string? SourceCode,
    string? Hash) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: read one computer's screen text, its hash, and the
/// housing under its dropdown. A missed id becomes
/// <see cref="Status.NOT_FOUND"/>; anything else that cannot be read
/// escapes toward the router, which answers 500.
/// </summary>
public sealed class ReadComputerCommand : Command<long, ReadComputerResult>
{
    private readonly IIcDevices _devices;

    public ReadComputerCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ReadComputerResult Execute(long computer)
    {
        try
        {
            var target = _devices.FindComputer(computer);
            var screen = target.Read();
            return new ReadComputerResult(
                Status.SUCCESS, new List<string>(), target.Name, target.Selected(), screen.SourceCode, screen.Hash);
        }
        catch (ThingNotFound notFound)
        {
            return new ReadComputerResult(
                Status.NOT_FOUND, new List<string> { notFound.Message }, null, 0, null, null);
        }
    }
}
