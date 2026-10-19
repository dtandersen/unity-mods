using System;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using StationeersAi.Core;
using StationeersAi.Game;
using StationeersAi.Entity;
using StationeersAi.Repository;
using Logger = StationeersAi.Core.Logger;
using GameStructure = Assets.Scripts.Objects.Structure;

namespace StationeersAi.Repository;

/// <summary>
/// Reads live pipe networks: air, size, and members. Runs on Unity's main
/// thread like the other live readers; a failed read is null or empty,
/// never fatal.
/// </summary>
public sealed class StationeersPipeNetworkInspector : IPipeNetworkInspector
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersPipeNetworkInspector(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }

    public PipeNetworkDetails? Describe(long pipeNetworkId)
    {
        try
        {
            return _threads.RunSync(() => DescribeInner(pipeNetworkId));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return null;
        }
    }

    public IReadOnlyList<PipeNetworkDetails> DescribeDevice(string? device)
    {
        try
        {
            return _threads.RunSync(() => DescribeDeviceInner(device));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new PipeNetworkDetails[0];
        }
    }

    private static PipeNetworkDetails? DescribeInner(long pipeNetworkId)
    {
        var active = PipeNetwork.AllPipeNetworks.Active().GetEnumerator();
        while (active.MoveNext())
        {
            var network = active.Current;
            if (network == null)
            {
                continue;
            }

            long id;
            try
            {
                id = network.ReferenceId;
            }
            catch (Exception)
            {
                continue;
            }

            if (id != pipeNetworkId)
            {
                continue;
            }

            try
            {
                return ReadPipeNetwork(network);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    private static IReadOnlyList<PipeNetworkDetails> DescribeDeviceInner(string? device)
    {
        if (string.IsNullOrWhiteSpace(device))
        {
            return new PipeNetworkDetails[0];
        }

        var matches = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<GameStructure>(),
            item => SafeName(item),
            item => item.ReferenceId,
            device);
        if (matches.Count == 1)
        {
            return FromStructure(matches[0]);
        }

        var tank = FindTank(device);
        if (tank != null)
        {
            var tankAir = ReadTank(tank);
            return tankAir == null
                ? new PipeNetworkDetails[0]
                : new[] { tankAir };
        }

        long id;
        if (long.TryParse(device.Trim(), out id))
        {
            var direct = DescribeInner(id);
            return direct == null
                ? new PipeNetworkDetails[0]
                : new[] { direct };
        }

        return new PipeNetworkDetails[0];
    }

    private static IReadOnlyList<PipeNetworkDetails> FromStructure(GameStructure structure)
    {
        var found = new List<PipeNetworkDetails>();
        var seen = new HashSet<long>();

        var pipe = structure as Pipe;
        if (pipe != null)
        {
            try
            {
                var own = pipe.PipeNetwork;
                if (own != null && seen.Add(own.ReferenceId))
                {
                    var read = ReadPipeNetwork(own);
                    if (read != null)
                    {
                        found.Add(read);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        var io = structure as DeviceInputOutput;
        if (io != null)
        {
            AddNetwork(found, seen, SafeNetwork(() => io.InputNetwork));
            AddNetwork(found, seen, SafeNetwork(() => io.InputNetwork2));
            AddNetwork(found, seen, SafeNetwork(() => io.OutputNetwork));
            AddNetwork(found, seen, SafeNetwork(() => io.OutputNetwork2));
        }

        var connector = structure as PortablesConnector;
        if (connector != null)
        {
            try
            {
                var occupant = connector.TankSlot?.Get<PortableAtmospherics>();
                var tankAir = occupant == null ? null : ReadTank(occupant);
                if (tankAir != null)
                {
                    found.Add(tankAir);
                }
            }
            catch (Exception)
            {
            }
        }

        return found;
    }

    private static void AddNetwork(List<PipeNetworkDetails> found, HashSet<long> seen, PipeNetwork? network)
    {
        if (network == null)
        {
            return;
        }

        long id;
        try
        {
            id = network.ReferenceId;
        }
        catch (Exception)
        {
            return;
        }

        if (!seen.Add(id))
        {
            return;
        }

        try
        {
            var read = ReadPipeNetwork(network);
            if (read != null)
            {
                found.Add(read);
            }
        }
        catch (Exception)
        {
        }
    }

    private static PipeNetwork? SafeNetwork(Func<PipeNetwork?> get)
    {
        try
        {
            return get();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static PortableAtmospherics? FindTank(string device)
    {
        try
        {
            var matches = ThingMatching.Match(
                UnityEngine.Object.FindObjectsOfType<PortableAtmospherics>(),
                item => SafeThingName(item),
                item => item.ReferenceId,
                device);
            return matches.Count == 1 ? matches[0] : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static PipeNetworkDetails? ReadPipeNetwork(PipeNetwork network)
    {
        Atmosphere? air = null;
        try
        {
            air = network.Atmosphere;
        }
        catch (Exception)
        {
        }

        if (air == null)
        {
            return null;
        }

        string? content = null;
        try
        {
            content = network.NetworkContentType.ToString();
        }
        catch (Exception)
        {
        }

        return Build(network.ReferenceId, content, air, MembersOf(network));
    }

    private static PipeNetworkDetails? ReadTank(PortableAtmospherics tank)
    {
        Atmosphere? air = null;
        try
        {
            air = tank.InternalAtmosphere;
        }
        catch (Exception)
        {
        }

        if (air == null)
        {
            return null;
        }

        string? content = null;
        try
        {
            content = "Tank " + tank.ContentType.ToString();
        }
        catch (Exception)
        {
            content = "Tank";
        }

        var members = new List<string>();
        try
        {
            var name = SafeThingName(tank);
            members.Add((name ?? "tank") + " #" + SafeThingId(tank));
        }
        catch (Exception)
        {
        }

        return Build(0, content, air, members);
    }

    private static PipeNetworkDetails Build(long id, string? content, Atmosphere air, IReadOnlyList<string> members)
    {
        double tempC = 0;
        double pressure = 0;
        double volume = 0;
        double moles = 0;
        try
        {
            tempC = air.Temperature.ToCelsius();
        }
        catch (Exception)
        {
        }

        try
        {
            pressure = air.PressureGassesAndLiquids.ToFloat();
        }
        catch (Exception)
        {
        }

        try
        {
            volume = air.Volume.ToFloat();
        }
        catch (Exception)
        {
        }

        try
        {
            moles = air.TotalMoles.ToFloat();
        }
        catch (Exception)
        {
        }

        GasMixture mix;
        try
        {
            mix = air.GasMixture;
        }
        catch (Exception)
        {
            return new PipeNetworkDetails(id, content, tempC, pressure, volume, moles, new GasMoles[0], members);
        }

        var gases = new List<GasMoles>();
        AddGas(gases, "Oxygen", () => mix.Oxygen.Quantity.ToFloat());
        AddGas(gases, "Nitrogen", () => mix.Nitrogen.Quantity.ToFloat());
        AddGas(gases, "CarbonDioxide", () => mix.CarbonDioxide.Quantity.ToFloat());
        AddGas(gases, "Hydrogen", () => mix.Hydrogen.Quantity.ToFloat());
        AddGas(gases, "Methane", () => mix.Methane.Quantity.ToFloat());
        AddGas(gases, "Pollutant", () => mix.Pollutant.Quantity.ToFloat());
        AddGas(gases, "Water", () => mix.Water.Quantity.ToFloat());
        AddGas(gases, "PollutedWater", () => mix.PollutedWater.Quantity.ToFloat());
        AddGas(gases, "NitrousOxide", () => mix.NitrousOxide.Quantity.ToFloat());
        AddGas(gases, "LiquidNitrogen", () => mix.LiquidNitrogen.Quantity.ToFloat());
        AddGas(gases, "LiquidOxygen", () => mix.LiquidOxygen.Quantity.ToFloat());
        AddGas(gases, "LiquidHydrogen", () => mix.LiquidHydrogen.Quantity.ToFloat());
        AddGas(gases, "LiquidMethane", () => mix.LiquidMethane.Quantity.ToFloat());
        AddGas(gases, "LiquidCarbonDioxide", () => mix.LiquidCarbonDioxide.Quantity.ToFloat());
        AddGas(gases, "LiquidPollutant", () => mix.LiquidPollutant.Quantity.ToFloat());
        AddGas(gases, "LiquidNitrousOxide", () => mix.LiquidNitrousOxide.Quantity.ToFloat());
        AddGas(gases, "Steam", () => mix.Steam.Quantity.ToFloat());
        AddGas(gases, "Helium", () => mix.Helium.Quantity.ToFloat());
        AddGas(gases, "Hydrazine", () => mix.Hydrazine.Quantity.ToFloat());
        AddGas(gases, "LiquidHydrazine", () => mix.LiquidHydrazine.Quantity.ToFloat());
        AddGas(gases, "LiquidAlcohol", () => mix.LiquidAlcohol.Quantity.ToFloat());
        AddGas(gases, "LiquidSodiumChloride", () => mix.LiquidSodiumChloride.Quantity.ToFloat());
        AddGas(gases, "Silanol", () => mix.Silanol.Quantity.ToFloat());
        AddGas(gases, "LiquidSilanol", () => mix.LiquidSilanol.Quantity.ToFloat());
        AddGas(gases, "HydrochloricAcid", () => mix.HydrochloricAcid.Quantity.ToFloat());
        AddGas(gases, "LiquidHydrochloricAcid", () => mix.LiquidHydrochloricAcid.Quantity.ToFloat());
        AddGas(gases, "Ozone", () => mix.Ozone.Quantity.ToFloat());
        AddGas(gases, "LiquidOzone", () => mix.LiquidOzone.Quantity.ToFloat());

        return new PipeNetworkDetails(id, content, tempC, pressure, volume, moles, gases, members);
    }

    private static IReadOnlyList<string> MembersOf(PipeNetwork network)
    {
        var members = new List<string>();
        try
        {
            foreach (var member in network.StructureList)
            {
                if (member == null || members.Count >= 40)
                {
                    break;
                }

                var thing = member as Thing;
                if (thing == null)
                {
                    continue;
                }

                try
                {
                    members.Add((thing.DisplayName ?? "unknown") + " #" + thing.ReferenceId);
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }

        try
        {
            foreach (var device in network.DeviceList)
            {
                if (device == null || members.Count >= 40)
                {
                    break;
                }

                var thing = device as Thing;
                if (thing == null)
                {
                    continue;
                }

                var label = SafeThingName(thing) + " #" + SafeThingId(thing);
                if (!members.Contains(label))
                {
                    members.Add(label);
                }
            }
        }
        catch (Exception)
        {
        }

        return members;
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

    private static string? SafeName(Thing thing)
    {
        try
        {
            return thing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafeThingName(Thing thing)
    {
        try
        {
            return thing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeThingId(Thing thing)
    {
        try
        {
            return thing.ReferenceId;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
