using System;
using System.Collections.Generic;

namespace StationeersAi.Repository
{
    /// <summary>Matches one named thing by reference id, exact label, or partial label.</summary>
    public static class ThingMatching
    {
        public static IReadOnlyList<T> Match<T>(IEnumerable<T> things, Func<T, string> name, Func<T, long> id, string device)
        {
            var matches = new List<T>();
            if (string.IsNullOrWhiteSpace(device))
            {
                return matches;
            }

            long reference;
            if (long.TryParse(device.Trim(), out reference))
            {
                foreach (var thing in things)
                {
                    long thingId;
                    try
                    {
                        thingId = id(thing);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (thingId == reference)
                    {
                        matches.Add(thing);
                    }
                }

                return matches;
            }

            var partial = new List<T>();
            foreach (var thing in things)
            {
                string label;
                try
                {
                    label = name(thing) ?? string.Empty;
                }
                catch (Exception)
                {
                    continue;
                }

                if (label.Equals(device, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(thing);
                }
                else if (label.IndexOf(device, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    partial.Add(thing);
                }
            }

            return matches.Count > 0 ? matches : partial;
        }
    }
}
