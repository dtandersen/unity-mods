using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Entity;
using StationeersAi.Mod;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Writes MIPS source to the IC editor of a named computer or laptop. Only
    /// fills the editor buffer (mirroring what the stationeer types); it never
    /// Exports to housings or chips. The write runs on Unity's main thread
    /// because the editor owns Unity UI text.
    /// </summary>
    public sealed class StationeersIcEditor : IIcEditor
    {
        public IcWriteResult Write(string device, string code)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => WriteInner(device, code ?? string.Empty));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new IcWriteResult(false, null, 0, "Editor write failed.");
            }
        }

        private static IcWriteResult WriteInner(string device, string code)
        {
            var boards = UnityEngine.Object.FindObjectsOfType<ProgrammableChipMotherboard>();
            var candidates = new List<(ProgrammableChipMotherboard Board, string Name)>();
            foreach (var board in boards)
            {
                if (board == null)
                {
                    continue;
                }

                string computer = null;
                try
                {
                    computer = board.ParentComputer?.AsThing()?.DisplayName;
                }
                catch (Exception)
                {
                }

                string own = null;
                try
                {
                    own = board.DisplayName;
                }
                catch (Exception)
                {
                }

                candidates.Add((board, computer ?? own ?? "unnamed editor"));
            }

            var matches = Match(candidates, device);
            if (matches.Count == 0)
            {
                return new IcWriteResult(false, null, 0, $"Computer '{device}' not found. Available: {Available(candidates)}");
            }

            if (matches.Count > 1)
            {
                return new IcWriteResult(false, null, 0, $"Several computers match '{device}': {Available(matches)}");
            }

            matches[0].Board.SetSourceCode(code);
            try
            {
                matches[0].Board.SendUpdate();
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
            }

            return new IcWriteResult(true, matches[0].Name, code.Split('\n').Length, null);
        }

        private static List<(ProgrammableChipMotherboard Board, string Name)> Match(
            List<(ProgrammableChipMotherboard Board, string Name)> candidates, string device)
        {
            long id;
            if (long.TryParse(device.Trim(), out id))
            {
                var byId = new List<(ProgrammableChipMotherboard Board, string Name)>();
                foreach (var candidate in candidates)
                {
                    long reference;
                    try
                    {
                        reference = candidate.Board.ReferenceId;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (reference == id)
                    {
                        byId.Add(candidate);
                    }
                }

                return byId;
            }

            var exact = new List<(ProgrammableChipMotherboard Board, string Name)>();
            var partial = new List<(ProgrammableChipMotherboard Board, string Name)>();
            foreach (var candidate in candidates)
            {
                if (candidate.Name.Equals(device, StringComparison.OrdinalIgnoreCase))
                {
                    exact.Add(candidate);
                }
                else if (candidate.Name.IndexOf(device, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    partial.Add(candidate);
                }
            }

            return exact.Count > 0 ? exact : partial;
        }

        private static string Available(List<(ProgrammableChipMotherboard Board, string Name)> candidates)
        {
            if (candidates.Count == 0)
            {
                return "none";
            }

            var names = new List<string>();
            foreach (var candidate in candidates)
            {
                if (!names.Contains(candidate.Name))
                {
                    names.Add(candidate.Name);
                }
            }

            return string.Join(", ", names.ToArray());
        }
    }
}
