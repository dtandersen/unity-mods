using System;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read the burned chip of one housing.</summary>
public sealed class ReadHousingCommand
{
    private readonly IIcDevices _devices;

    public ReadHousingCommand(IIcDevices devices)
    {
        _devices = devices;
    }

    public IcReadResult Execute(long device)
    {
        IHousing target;
        try
        {
            target = _devices.FindHousing(device);
        }
        catch (ThingNotFound notFound)
        {
            return new IcReadResult(false, null, null, notFound.Message);
        }
        catch (Exception)
        {
            return new IcReadResult(false, null, null, "Housing lookup failed.");
        }

        return target.Read();
    }
}
