using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Motherboards;
using UnityEngine;

namespace StationeersAi.Repository
{
    /// <summary>Finds IC editor boards by computer label, board label, or reference id.</summary>
    internal static class ComputerBoards
    {
        internal sealed class Board
        {
            public Board(ProgrammableChipMotherboard motherboard, string name)
            {
                Motherboard = motherboard;
                Name = name;
            }

            public ProgrammableChipMotherboard Motherboard { get; }

            public string Name { get; }
        }

        internal static IReadOnlyList<Board> Match(string device)
        {
            return ThingMatching.Match(All(), board => board.Name, board => board.Motherboard.ReferenceId, device);
        }

        internal static string Names(IReadOnlyList<Board> boards)
        {
            if (boards.Count == 0)
            {
                return "none";
            }

            var names = new List<string>();
            foreach (var board in boards)
            {
                if (!names.Contains(board.Name))
                {
                    names.Add(board.Name);
                }
            }

            return string.Join(", ", names.ToArray());
        }

        internal static string Available()
        {
            return Names(All());
        }

        private static IReadOnlyList<Board> All()
        {
            var boards = new List<Board>();
            foreach (var motherboard in UnityEngine.Object.FindObjectsOfType<ProgrammableChipMotherboard>())
            {
                if (motherboard == null)
                {
                    continue;
                }

                string computer = null;
                try
                {
                    computer = motherboard.ParentComputer?.AsThing()?.DisplayName;
                }
                catch (Exception)
                {
                }

                string own = null;
                try
                {
                    own = motherboard.DisplayName;
                }
                catch (Exception)
                {
                }

                boards.Add(new Board(motherboard, computer ?? own ?? "unnamed editor"));
            }

            return boards;
        }
    }
}
