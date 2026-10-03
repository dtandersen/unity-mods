using System;
using System.Collections.Generic;
using Assets.Scripts.Objects.Motherboards;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>Finds IC editor boards by computer label, board label, or reference id.</summary>
internal static class ComputerBoards
{
    internal sealed class Board
    {
        public Board(ProgrammableChipMotherboard motherboard, string name, long computerId)
        {
            Motherboard = motherboard;
            Name = name;
            ComputerId = computerId;
        }

        public ProgrammableChipMotherboard Motherboard { get; }

        public string Name { get; }

        public long ComputerId { get; }
    }

    internal static IReadOnlyList<Board> Match(string computer)
    {
        long thing;
        if (!string.IsNullOrWhiteSpace(computer) && long.TryParse(computer.Trim(), out thing))
        {
            var matches = new List<Board>();
            foreach (var board in All())
            {
                long reference;
                try
                {
                    reference = board.Motherboard.ReferenceId;
                }
                catch (Exception)
                {
                    continue;
                }

                if (reference == thing || board.ComputerId == thing)
                {
                    matches.Add(board);
                }
            }

            return matches;
        }

        return ThingMatching.Match(All(), board => board.Name, board => board.Motherboard.ReferenceId, computer);
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
            long computerId = 0;
            try
            {
                var computerThing = motherboard.ParentComputer?.AsThing();
                computer = computerThing?.DisplayName;
                computerId = computerThing != null ? computerThing.ReferenceId : 0;
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

            boards.Add(new Board(motherboard, computer ?? own ?? "unnamed editor", computerId));
        }

        return boards;
    }
}
