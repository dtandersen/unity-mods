using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>The housing to point at and the computer whose dropdown moves.</summary>
public sealed record SelectHousingRequest(long Computer, long Device);

/// <summary>
/// Domain operation: point the dropdown at one housing and answer with its
/// name. A missed id becomes <see cref="Status.NOT_FOUND"/>; a machine that
/// fails outright raises.
/// </summary>
public sealed class SelectHousingCommand : Command<SelectHousingRequest, ComputerActionResult>
{
    private readonly IIcDevices _devices;

    public SelectHousingCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public ComputerActionResult Execute(SelectHousingRequest request)
    {
        try
        {
            var target = _devices.FindComputer(request.Computer);
            target.Select(request.Device);
            return ComputerAnswers.Accept(_devices.FindHousing(request.Device).Name, 0);
        }
        catch (ThingNotFound notFound)
        {
            return ComputerAnswers.NotFound(notFound.Message);
        }
    }
}
