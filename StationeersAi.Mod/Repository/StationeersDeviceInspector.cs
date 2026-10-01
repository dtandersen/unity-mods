using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using StationeersAi.Entity;
using StationeersAi.Formatting;
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
                ReadSlots(target as ILogicable),
                ReadHousingDevices(target),
                ReadChip(target));
        }

        /// <summary>
        /// The running chip's hover-visible state: error line and type, the
        /// display value, and a hash of its code (never the code itself).
        /// Null unless a chip is slotted.
        /// </summary>
        private static ChipState ReadChip(Device target)
        {
            try
            {
                var housing = target as CircuitHousing;
                var chip = housing?._ProgrammableChipSlot?.Get<ProgrammableChip>();
                if (chip == null)
                {
                    return null;
                }

                string errorType = null;
                string errorLine = null;
                try
                {
                    if (chip.CompilationError)
                    {
                        errorType = CompileErrorType(chip) ?? "compile error";
                        errorLine = CompileErrorLine(chip);
                    }
                    else if (!string.IsNullOrEmpty(chip.ErrorTypeString) &&
                        !chip.ErrorTypeString.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        errorType = chip.ErrorTypeString;
                        errorLine = string.IsNullOrEmpty(chip.ErrorLineNumberString) ? null : chip.ErrorLineNumberString;
                    }
                }
                catch (Exception)
                {
                }

                double? display = null;
                try
                {
                    var logicable = target as ILogicable;
                    if (logicable != null && logicable.CanLogicRead(LogicType.Setting))
                    {
                        display = logicable.GetLogicValue(LogicType.Setting);
                    }
                }
                catch (Exception)
                {
                }

                string hash = null;
                try
                {
                    var source = chip.GetSourceCode();
                    hash = CodeHash.Hash(source != null ? source.ToString() : string.Empty);
                }
                catch (Exception)
                {
                }

                return new ChipState(errorType, errorLine, display, hash);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CompileErrorType(ProgrammableChip chip)
        {
            var field = typeof(ProgrammableChip).GetField("_compileErrorType", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(chip)?.ToString();
        }

        private static string CompileErrorLine(ProgrammableChip chip)
        {
            var field = typeof(ProgrammableChip).GetField("_compileErrorLineNumber", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field?.GetValue(chip);
            return value != null ? value.ToString() : null;
        }

        private static IReadOnlyList<HousingDeviceEntry> ReadHousingDevices(Device target)
        {
            var holder = target as ICircuitHolder;
            if (holder == null)
            {
                return null;
            }

            List<LogicBinding> bindings;
            try
            {
                bindings = holder.GetLogicBindings();
            }
            catch (Exception)
            {
                return null;
            }

            if (bindings == null)
            {
                return null;
            }

            var entries = new List<HousingDeviceEntry>();
            foreach (var binding in bindings)
            {
                if (binding == null || string.IsNullOrEmpty(binding.Header))
                {
                    continue;
                }

                if (binding.Header == "db")
                {
                    entries.Add(new HousingDeviceEntry("db", SafeName(target) ?? "housing", SafeId(target)));
                    continue;
                }

                int index;
                if (!binding.Header.StartsWith("d") || !int.TryParse(binding.Header.Substring(1), out index))
                {
                    continue;
                }

                Thing thing = null;
                try
                {
                    var logicable = holder.GetLogicableFromIndex(index);
                    thing = logicable != null ? logicable.GetAsThing : null;
                }
                catch (Exception)
                {
                }

                if (thing == null)
                {
                    var screwed = ScrewedIn(target, index);
                    if (screwed != null)
                    {
                        entries.Add(new HousingDeviceEntry(binding.Header, screwed.Item1 + " (off network)", screwed.Item2));
                    }
                    else
                    {
                        entries.Add(new HousingDeviceEntry(binding.Header, "empty", 0));
                    }

                    continue;
                }

                string label = null;
                try
                {
                    label = thing.DisplayName;
                }
                catch (Exception)
                {
                }

                long id = 0;
                try
                {
                    id = thing.ReferenceId;
                }
                catch (Exception)
                {
                }

                entries.Add(new HousingDeviceEntry(binding.Header, label ?? "unknown", id));
            }

            return entries;
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

        /// <summary>The thing screwed into a housing screw, even when the chip cannot see it.</summary>
        private static Tuple<string, long> ScrewedIn(Device target, int index)
        {
            try
            {
                var housing = target as CircuitHousing;
                if (housing == null || housing.Devices == null || index < 0 || index >= housing.Devices.Length)
                {
                    return null;
                }

                var thing = housing.Devices[index]?.GetAsThing;
                if (thing == null)
                {
                    return null;
                }

                return Tuple.Create(thing.DisplayName ?? "unknown", thing.ReferenceId);
            }
            catch (Exception)
            {
                return null;
            }
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
