using System;
using System.Collections.Generic;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Finds the IC editor board a label or thing id names and builds one
/// <see cref="StationeersComputer"/> for the single match. Reads Unity's
/// objects on the main thread, so the computer it hands back already names
/// one editor.
/// </summary>
public sealed class StationeersComputerRepository : IComputerRepository
{
    /// <summary>One IC editor board and the computer it belongs to.</summary>
    private sealed class Board
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

    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersComputerRepository(IMainThread threads, Logger log)
    {
        _threads = threads;
        _log = log;
    }

    public ComputerLookup Find(string? computer)
    {
        try
        {
            return _threads.RunSync(() => FindInner(computer));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new ComputerLookup(null, "Computer lookup failed.");
        }
    }

    private ComputerLookup FindInner(string? computer)
    {
        var boards = Match(computer);
        if (boards.Count == 0)
        {
            return new ComputerLookup(null, $"Computer '{computer}' not found. Available: {Names(All())}");
        }

        if (boards.Count > 1)
        {
            return new ComputerLookup(null, $"Several computers match '{computer}': {Names(boards)}");
        }

        var board = boards[0];
        return new ComputerLookup(new StationeersComputer(board.Motherboard, board.Name, _threads, _log), null);
    }

    /// <summary>Finds IC editor boards by computer label, board label, or reference id.</summary>
    private static IReadOnlyList<Board> Match(string? computer)
    {
        long thing;
        if (computer is not null && !string.IsNullOrWhiteSpace(computer) && long.TryParse(computer.Trim(), out thing))
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

    private static string Names(IReadOnlyList<Board> boards)
    {
        var names = new List<string>();
        foreach (var board in boards)
        {
            if (!names.Contains(board.Name))
            {
                names.Add(board.Name);
            }
        }

        return names.Count == 0 ? "none" : string.Join(", ", names.ToArray());
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

            string? computer = null;
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

            string? own = null;
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
