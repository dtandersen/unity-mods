using System;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read the burned chip of one housing.</summary>
public sealed class ReadHousingCommand
{
    private readonly IHousingRepository _housings;

    public ReadHousingCommand(IHousingRepository housings)
    {
        _housings = housings;
    }

    public IcReadResult Execute(long device)
    {
        IHousing target;
        try
        {
            target = _housings.Find(device);
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
