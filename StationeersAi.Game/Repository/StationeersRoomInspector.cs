using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Reads a room's atmospherics from the live game: type, temperature,
/// pressure, volume, and the gas mix. Runs on Unity's main thread like the
/// other live readers; a failed read is null, never fatal.
/// </summary>
public sealed class StationeersRoomInspector : IRoomInspector
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersRoomInspector(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }

    public RoomAtmosphere Describe(long roomId)
    {
        try
        {
            return _threads.RunSync(() => DescribeInner(roomId));
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return null;
        }
    }

    private static RoomAtmosphere DescribeInner(long roomId)
    {
        if (RoomController.World == null)
        {
            return null;
        }

        var room = RoomController.Get(roomId);
        if (room == null)
        {
            return null;
        }

        var mix = room.GasMixture;

        var gases = new List<GasMoles>();
        AddGas(gases, "Oxygen", () => mix.Oxygen.Quantity.ToFloat());
        AddGas(gases, "Nitrogen", () => mix.Nitrogen.Quantity.ToFloat());
        AddGas(gases, "CarbonDioxide", () => mix.CarbonDioxide.Quantity.ToFloat());
        AddGas(gases, "Hydrogen", () => mix.Hydrogen.Quantity.ToFloat());
        AddGas(gases, "Methane", () => mix.Methane.Quantity.ToFloat());
        AddGas(gases, "Pollutant", () => mix.Pollutant.Quantity.ToFloat());
        AddGas(gases, "Water", () => mix.Water.Quantity.ToFloat());
        AddGas(gases, "LiquidNitrogen", () => mix.LiquidNitrogen.Quantity.ToFloat());
        AddGas(gases, "LiquidOxygen", () => mix.LiquidOxygen.Quantity.ToFloat());
        AddGas(gases, "LiquidHydrogen", () => mix.LiquidHydrogen.Quantity.ToFloat());
        AddGas(gases, "LiquidMethane", () => mix.LiquidMethane.Quantity.ToFloat());
        AddGas(gases, "LiquidCarbonDioxide", () => mix.LiquidCarbonDioxide.Quantity.ToFloat());
        AddGas(gases, "LiquidPollutant", () => mix.LiquidPollutant.Quantity.ToFloat());
        AddGas(gases, "LiquidNitrousOxide", () => mix.LiquidNitrousOxide.Quantity.ToFloat());
        AddGas(gases, "Steam", () => mix.Steam.Quantity.ToFloat());
        AddGas(gases, "NitrousOxide", () => mix.NitrousOxide.Quantity.ToFloat());
        AddGas(gases, "Helium", () => mix.Helium.Quantity.ToFloat());

        return new RoomAtmosphere(
            room.RoomId,
            room.RoomType.ToString(),
            room.Temperature.ToCelsius(),
            room.Pressure.ToFloat(),
            room.Volume.ToFloat(),
            gases);
    }

    private static void AddGas(List<GasMoles> gases, string name, Func<double> moles)
    {
        double amount;
        try
        {
            amount = moles();
        }
        catch (Exception)
        {
            return;
        }

        if (amount > 0)
        {
            gases.Add(new GasMoles(name, amount));
        }
    }
}
