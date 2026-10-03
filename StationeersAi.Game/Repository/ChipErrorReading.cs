using System;
using System.Reflection;
using Assets.Scripts.Objects.Electrical;

namespace StationeersAi.Repository;

/// <summary>Reads a burned chip's compile error, which the game keeps private.</summary>
internal static class ChipErrorReading
{
    internal static string CompileErrorType(ProgrammableChip chip)
    {
        try
        {
            if (chip == null || !chip.CompilationError)
            {
                return null;
            }

            var field = typeof(ProgrammableChip).GetField("_compileErrorType", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(chip)?.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static string CompileErrorLine(ProgrammableChip chip)
    {
        try
        {
            if (chip == null)
            {
                return null;
            }

            var field = typeof(ProgrammableChip).GetField("_compileErrorLineNumber", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field?.GetValue(chip);
            return value != null ? value.ToString() : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
