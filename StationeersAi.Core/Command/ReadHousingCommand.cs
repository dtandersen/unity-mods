using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>
/// Domain operation: read the program burned on one housing's chip. Faults
/// travel to the caller: a missed id raises <see cref="ThingNotFound"/>, an
/// unreadable chip raises after it.
/// </summary>
public sealed class ReadHousingCommand
{
    private readonly IIcDevices _devices;

    public ReadHousingCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public Program Execute(long device)
    {
        return _devices.FindHousing(device).Read();
    }
}
