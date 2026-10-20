using Sage.Entity;

namespace Sage.Repository;

/// <summary>
/// Finds the IC devices a stationeer built: the computers that program chips
/// and the housings that hold them. Each lookup builds one device object for
/// the thing id it is given. Tests fill this with
/// <see cref="InMemoryIcDevices"/>; the mod fills it with live things.
/// </summary>
public interface IIcDevices
{
    /// <summary>Resolves one computer. Throws <see cref="ThingNotFound"/> when no board carries the id.</summary>
    IComputer FindComputer(long computerId);

    /// <summary>Resolves one housing. Throws <see cref="ThingNotFound"/> when no housing carries the id.</summary>
    IHousing FindHousing(long thingId);
}
