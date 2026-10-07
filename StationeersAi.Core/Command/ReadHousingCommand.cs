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
        var found = _housings.Find(device);
        var target = found.Housing;
        if (target == null)
        {
            return new IcReadResult(false, null, null, found.Error ?? "Housing not found.");
        }

        return target.Read();
    }
}
