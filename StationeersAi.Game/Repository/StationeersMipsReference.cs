using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Reads the IC10 instruction reference from the game's editor help (usage
/// examples and localized descriptions per instruction). Read live on every
/// access; the tables are static game data, so no snapshot is needed.
/// A failed entry is skipped, never fatal.
/// </summary>
public sealed class StationeersMipsReference : IMipsReference
{
    private readonly Logger _log;

    public StationeersMipsReference(Logger log)
    {
        _log = log;
    }

    public IReadOnlyList<Instruction>? Instructions => Read();

    private IReadOnlyList<Instruction> Read()
    {
        var instructions = new List<Instruction>();
        foreach (ScriptCommand command in Enum.GetValues(typeof(ScriptCommand)))
        {
            try
            {
                if (LogicBase.IsDeprecated(command))
                {
                    continue;
                }

                instructions.Add(new Instruction(
                    command.ToString(),
                    ProgrammableChip.StripColorTags(ProgrammableChip.GetCommandExample(command)),
                    ProgrammableChip.GetCommandDescription(command)));
            }
            catch (Exception ex)
            {
                _log.Info($"mips reference skipped {command}: {ex.Message}");
            }
        }

        instructions.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return instructions;
    }
}
