using System;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using StationeersAi.Entity;
using StationeersAi.Mod;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Describes one live device: on/off state, every readable logic setting
    /// with its live value and writability, and slots with occupants.
    /// Snapshotted on Unity's main thread; unreadable entries are skipped,
    /// never fatal.
    /// </summary>
    public sealed class StationeersDeviceInspector : IDeviceInspector
    {
        public DeviceDetails Describe(string device)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => DescribeInner(device));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return null;
            }
        }

        private static DeviceDetails DescribeInner(string device)
        {
            var matches = ThingMatching.Match(
                UnityEngine.Object.FindObjectsOfType<Device>(),
                item => SafeName(item),
                item => item.ReferenceId,
                device);
            if (matches.Count != 1)
            {
                return null;
            }

            var target = matches[0];
            long network = 0;
            try
            {
                var networkObject = target.DataCableNetwork;
                network = networkObject != null ? networkObject.ReferenceId : 0;
            }
            catch (Exception)
            {
            }

            bool? isOn = null;
            try
            {
                if (target.HasOnOffState)
                {
                    isOn = target.OnOff;
                }
            }
            catch (Exception)
            {
            }

            return new DeviceDetails(
                SafeName(target),
                SafeType(target),
                SafeId(target),
                network,
                isOn,
                ReadLogic(target as ILogicable),
                ReadSlots(target as ILogicable));
        }

        private static IReadOnlyList<DeviceLogicValue> ReadLogic(ILogicable logicable)
        {
            var values = new List<DeviceLogicValue>();
            if (logicable == null)
            {
                return values;
            }

            foreach (LogicType type in Enum.GetValues(typeof(LogicType)))
            {
                try
                {
                    if (LogicBase.IsDeprecated(type) || !logicable.CanLogicRead(type))
                    {
                        continue;
                    }

                    values.Add(new DeviceLogicValue(type.ToString(), logicable.GetLogicValue(type), logicable.CanLogicWrite(type)));
                }
                catch (Exception)
                {
                }
            }

            return values;
        }

        private static IReadOnlyList<DeviceSlotInfo> ReadSlots(ILogicable logicable)
        {
            var slots = new List<DeviceSlotInfo>();
            if (logicable == null)
            {
                return slots;
            }

            int count;
            try
            {
                count = logicable.TotalSlots;
            }
            catch (Exception)
            {
                return slots;
            }

            for (var index = 0; index < count; index++)
            {
                Slot slot;
                try
                {
                    slot = logicable.GetSlot(index);
                }
                catch (Exception)
                {
                    continue;
                }

                if (slot == null)
                {
                    continue;
                }

                string name = null;
                try
                {
                    name = slot.DisplayName;
                }
                catch (Exception)
                {
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = slot.Type.ToString();
                }

                string occupant = "empty";
                try
                {
                    var held = slot.Get();
                    if (held != null)
                    {
                        occupant = StationeersItemRepository.SafeDisplayName(held) ?? held.PrefabName ?? "unknown";
                    }
                }
                catch (Exception)
                {
                }

                slots.Add(new DeviceSlotInfo(index, name, occupant));
            }

            return slots;
        }

        private static string SafeName(Device device)
        {
            try
            {
                return device.DisplayName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SafeType(Device device)
        {
            try
            {
                return device.PrefabName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static long SafeId(Device device)
        {
            try
            {
                return device.ReferenceId;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
