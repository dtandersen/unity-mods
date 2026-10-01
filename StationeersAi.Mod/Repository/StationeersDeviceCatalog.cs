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
    /// Finds built things: devices, pipes, walls. Logic speakers are the ones
    /// implementing <c>ILogicable</c>. Snapshotted on Unity's main thread;
    /// dying things are skipped, never fatal.
    /// </summary>
    public sealed class StationeersDeviceCatalog : IDeviceCatalog
    {
        public IReadOnlyList<DeviceInfo> Search(string query, long? network, bool? logic)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => SearchInner(query, network, logic));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new DeviceInfo[0];
            }
        }

        private static IReadOnlyList<DeviceInfo> SearchInner(string query, long? network, bool? logic)
        {
            var devices = new List<DeviceInfo>();
            foreach (var structure in UnityEngine.Object.FindObjectsOfType<Assets.Scripts.Objects.Structure>())
            {
                var info = Describe(structure);
                if (info != null &&
                    Matches(info, query) &&
                    (!network.HasValue || info.NetworkId == network.Value) &&
                    (!logic.HasValue || info.Logic == logic.Value))
                {
                    devices.Add(info);
                }
            }

            return devices;
        }

        private static DeviceInfo Describe(Assets.Scripts.Objects.Structure structure)
        {
            try
            {
                if (structure == null)
                {
                    return null;
                }

                long network = 0;
                try
                {
                    var device = structure as Device;
                    var networkObject = device != null ? device.DataCableNetwork : null;
                    network = networkObject != null ? networkObject.ReferenceId : 0;
                }
                catch (Exception)
                {
                }

                return DescribeThing(structure, network, structure is ILogicable);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DeviceInfo DescribeThing(Thing thing, long network, bool logic)
        {
            try
            {
                return new DeviceInfo(thing.DisplayName, thing.PrefabName, thing.ReferenceId, network, logic);
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
