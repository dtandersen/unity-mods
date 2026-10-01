using System;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using StationeersAi.Entity;
using StationeersAi.Mod;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Finds live logic devices: everything in the world, or one computer's
    /// data network (the same list its IC editor dropdown shows). Snapshotted
    /// on Unity's main thread; dying devices are skipped, never fatal.
    /// </summary>
    public sealed class StationeersDeviceCatalog : IDeviceCatalog
    {
        public IReadOnlyList<DeviceInfo> Search(string query, string computer)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => SearchInner(query, computer));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new DeviceInfo[0];
            }
        }

        private static IReadOnlyList<DeviceInfo> SearchInner(string query, string computer)
        {
            var devices = new List<DeviceInfo>();
            if (string.IsNullOrWhiteSpace(computer))
            {
                foreach (var device in UnityEngine.Object.FindObjectsOfType<Device>())
                {
                    var info = Describe(device);
                    if (info != null && Matches(info, query))
                    {
                        devices.Add(info);
                    }
                }

                return devices;
            }

            var matches = ComputerBoards.Match(computer);
            if (matches.Count != 1)
            {
                return devices;
            }

            long network = 0;
            try
            {
                var networkObject = matches[0].Motherboard.ParentComputer?.DataCableNetwork;
                network = networkObject != null ? networkObject.ReferenceId : 0;
            }
            catch (Exception)
            {
            }

            List<ILogicable> networkDevices;
            try
            {
                networkDevices = matches[0].Motherboard.ParentComputer?.DeviceList();
            }
            catch (Exception)
            {
                return devices;
            }

            if (networkDevices == null)
            {
                return devices;
            }

            foreach (var logicable in networkDevices)
            {
                Thing thing = null;
                try
                {
                    thing = logicable?.GetAsThing;
                }
                catch (Exception)
                {
                }

                if (thing == null)
                {
                    continue;
                }

                var info = DescribeThing(thing, network);
                if (info != null && Matches(info, query))
                {
                    devices.Add(info);
                }
            }

            return devices;
        }

        private static DeviceInfo Describe(Device device)
        {
            try
            {
                if (device == null || !(device is ILogicable))
                {
                    return null;
                }

                long network = 0;
                try
                {
                    var networkObject = device.DataCableNetwork;
                    network = networkObject != null ? networkObject.ReferenceId : 0;
                }
                catch (Exception)
                {
                }

                return DescribeThing(device, network);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DeviceInfo DescribeThing(Thing thing, long network)
        {
            try
            {
                return new DeviceInfo(thing.DisplayName, thing.PrefabName, thing.ReferenceId, network);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool Matches(DeviceInfo info, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return (info.Name != null && info.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (info.Type != null && info.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
