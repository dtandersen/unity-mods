using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read a housing's burned chip code.</summary>
public sealed class ReadHousingCommand
{
    private readonly IIcHousing _housing;

    public ReadHousingCommand(IIcHousing housing)
    {
        _housing = housing;
    }

    public IcReadResult Execute(string? device)
    {
        return _housing.Read(device);
    }
}
