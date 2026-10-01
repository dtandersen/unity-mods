using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Entity;
using StationeersAi.Mod;
using StationeersAi.Repository;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Reads the IC10 instruction reference from the game's editor help (usage
    /// examples and localized descriptions per instruction). Snapshotted on the
    /// calling (main) thread; a failed entry is skipped, never fatal.
    /// </summary>
    public sealed class StationeersMipsReference : IMipsReference
    {
        public StationeersMipsReference()
        {
            Instructions = Read();
        }

        public IReadOnlyList<MipsInstruction> Instructions { get; }

        private static IReadOnlyList<MipsInstruction> Read()
        {
            var instructions = new List<MipsInstruction>();
            foreach (ScriptCommand command in Enum.GetValues(typeof(ScriptCommand)))
            {
                try
                {
                    if (LogicBase.IsDeprecated(command))
                    {
                        continue;
                    }

                    instructions.Add(new MipsInstruction(
                        command.ToString(),
                        ProgrammableChip.StripColorTags(ProgrammableChip.GetCommandExample(command)),
                        ProgrammableChip.GetCommandDescription(command)));
                }
                catch (Exception ex)
                {
                    BepInExPlugin.Log.Log($"mips reference skipped {command}: {ex.Message}");
                }
            }

            instructions.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return instructions;
        }
    }
}
