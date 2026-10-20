using System;
using System.Linq;
using Sage.Core;
using Sage.Game;
using Logger = Sage.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using Sage.Entity;
using Sage.Repository;
using UnityEngine;

namespace Sage.Repository;

/// <summary>
/// Finds built structures: machines, pipes, walls. Logic speakers are the ones
/// implementing <c>ILogicable</c>. Snapshotted on Unity's main thread;
/// dying things are skipped, never fatal.
/// </summary>
public sealed class StationeersStructureCatalog : IStructureCatalog
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersStructureCatalog(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }
    public IReadOnlyList<StructureInfo> Search(string? query, long? powerNetworkId, long? dataNetworkId, bool? logic, long? room)
    {
        try
        {
            return _threads.RunSync(() => SearchInner(query, powerNetworkId, dataNetworkId, logic, room));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new StructureInfo[0];
        }
    }

    private IReadOnlyList<StructureInfo> SearchInner(string? query, long? powerNetworkId, long? dataNetworkId, bool? logic, long? room)
    {
        var structures = new List<StructureInfo>();
        foreach (var structure in UnityEngine.Object.FindObjectsOfType<Assets.Scripts.Objects.Structure>())
        {
            var info = Describe(structure);
            if (info != null &&
                Matches(info, query) &&
                (!powerNetworkId.HasValue || info.Connections.Any(c => c.Kind == "power" && c.NetworkId == powerNetworkId.Value)) &&
                (!dataNetworkId.HasValue || info.Connections.Any(c => c.Kind == "data" && c.NetworkId == dataNetworkId.Value)) &&
                (!logic.HasValue || info.Logic == logic.Value) &&
                (!room.HasValue || info.RoomId == room.Value))
            {
                structures.Add(info);
            }
        }

        return structures;
    }

    private static StructureInfo? Describe(Assets.Scripts.Objects.Structure structure)
    {
        try
        {
            if (structure == null)
            {
                return null;
            }

            var connections = ReadConnections(structure);
            return DescribeThing(structure, connections, structure is ILogicable, RoomOf(structure));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static List<NetworkConnection> ReadConnections(Assets.Scripts.Objects.Structure? structure)
    {
        var connections = new List<NetworkConnection>();
        if (structure == null)
        {
            return connections;
        }

        var wired = structure as Device;
        if (wired != null)
        {
            AddCableConnections(connections, wired);
            AddPipeConnections(connections, structure, wired);
            AddChuteConnections(connections, structure, wired);
        }
        else
        {
            AddPipeConnections(connections, structure, null);
            AddChuteConnections(connections, structure, null);
        }

        return connections;
    }

    private static void AddCableConnections(List<NetworkConnection> connections, Device wired)
    {
        try
        {
            var power = wired.PowerCables;
            if (power != null)
            {
                for (var i = 0; i < power.Length; i++)
                {
                    try
                    {
                        var network = power[i] != null ? power[i].CableNetwork : null;
                        if (network != null)
                        {
                            connections.Add(new NetworkConnection("power", i, network.ReferenceId));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        try
        {
            var data = wired.DataCables;
            if (data != null)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    try
                    {
                        var network = data[i] != null ? data[i].CableNetwork : null;
                        if (network != null)
                        {
                            connections.Add(new NetworkConnection("data", i, network.ReferenceId));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Every pipe mouth: a lone pipe joins one network; a device with
    /// mouths reports input, input2, output, output2 in that order
    /// (output2 often carries liquids, e.g. the furnace liquid out).
    /// Anything else on pipe networks joins by network id.
    /// </summary>
    private static void AddPipeConnections(List<NetworkConnection> connections, Assets.Scripts.Objects.Structure structure, Device? wired)
    {
        try
        {
            var pipe = structure as Pipe;
            if (pipe != null)
            {
                try
                {
                    var own = pipe.PipeNetwork;
                    if (own != null)
                    {
                        connections.Add(new NetworkConnection("pipe", 0, own.ReferenceId));
                    }
                }
                catch (Exception)
                {
                }

                return;
            }
        }
        catch (Exception)
        {
        }

        try
        {
            var io = structure as DeviceInputOutput;
            if (io != null)
            {
                AddPipe(connections, "input", 0, SafePipeId(() => io.InputNetwork));
                AddPipe(connections, "input2", 1, SafePipeId(() => io.InputNetwork2));
                AddPipe(connections, "output", 2, SafePipeId(() => io.OutputNetwork));
                AddPipe(connections, "output2", 3, SafePipeId(() => io.OutputNetwork2));
                return;
            }
        }
        catch (Exception)
        {
        }

        if (wired == null)
        {
            return;
        }

        try
        {
            var joined = wired.ConnectedPipeNetworks;
            if (joined == null)
            {
                return;
            }

            var index = 0;
            foreach (var network in joined)
            {
                try
                {
                    if (network == null)
                    {
                        continue;
                    }

                    var id = network.ReferenceId;
                    if (connections.Any(c => c.Kind == "pipe" && c.NetworkId == id))
                    {
                        continue;
                    }

                    connections.Add(new NetworkConnection("pipe", index++, id));
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private static void AddPipe(List<NetworkConnection> connections, string port, int index, long? networkId)
    {
        if (networkId.HasValue && networkId.Value != 0)
        {
            connections.Add(new NetworkConnection("pipe", index, networkId.Value, port));
        }
    }

    private static long? SafePipeId(Func<object?> get)
    {
        try
        {
            var network = get() as Assets.Scripts.Networks.PipeNetwork;
            return network == null ? (long?)null : network.ReferenceId;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void AddChuteConnections(List<NetworkConnection> connections, Assets.Scripts.Objects.Structure structure, Device? wired)
    {
        try
        {
            var chute = structure as Chute;
            if (chute != null)
            {
                try
                {
                    var own = chute.ChuteNetwork;
                    if (own != null)
                    {
                        connections.Add(new NetworkConnection("chute", 0, own.ReferenceId));
                    }
                }
                catch (Exception)
                {
                }

                return;
            }
        }
        catch (Exception)
        {
        }

        if (wired == null)
        {
            return;
        }

        try
        {
            var joined = wired.ConnectedChuteNetworks;
            if (joined == null)
            {
                return;
            }

            var index = 0;
            foreach (var network in joined)
            {
                try
                {
                    if (network == null)
                    {
                        continue;
                    }

                    connections.Add(new NetworkConnection("chute", index++, network.ReferenceId));
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private static long RoomOf(Thing thing)
    {
        try
        {
            var controller = RoomController.World;
            if (controller == null || thing == null)
            {
                return 0;
            }

            var room = controller.GetRoom(thing.GridPosition);
            return room != null ? room.RoomId : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static StructureInfo? DescribeThing(Thing thing, List<NetworkConnection> connections, bool logic, long room)
    {
        try
        {
            var center = SafeCenter(thing);
            return new StructureInfo(thing.DisplayName, thing.PrefabName, thing.ReferenceId, connections, logic, room, new StructurePosition(center.x, center.y, center.z));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Vector3 SafeCenter(Thing thing)
    {
        try
        {
            return thing != null ? thing.CenterPosition : Vector3.zero;
        }
        catch (Exception)
        {
            return Vector3.zero;
        }
    }

    private static bool Matches(StructureInfo info, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return (info.Name != null && info.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
            (info.PrefabName != null && info.PrefabName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
