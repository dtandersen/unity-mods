using System;

namespace StationeersAi.Entity;

/// <summary>
/// The rule in front of an editor write: the hash you name must be the hash
/// of the chip under the dropdown. It stops a write aimed at one chip from
/// overwriting the source that points at another.
/// </summary>
public static class WriteGuard
{
    /// <summary>
    /// The reason to refuse, or null when the write may go ahead.
    /// <paramref name="pointedDevice"/> is the housing under the dropdown,
    /// <paramref name="burnedHash"/> the hash of the program on its chip, and
    /// <paramref name="chipHash"/> the hash the caller named.
    /// </summary>
    public static string? Refusal(string? pointedDevice, string burnedHash, string? chipHash)
    {
        if (string.IsNullOrEmpty(pointedDevice))
        {
            return "The computer points at no housing. Pick the housing first.";
        }

        if (!burnedHash.Equals(chipHash ?? string.Empty, StringComparison.OrdinalIgnoreCase))
        {
            return $"Chip on {pointedDevice} holds {burnedHash}, not {chipHash}. Read it, then write with its hash.";
        }

        return null;
    }
}
