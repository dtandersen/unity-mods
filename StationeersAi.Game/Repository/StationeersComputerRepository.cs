using System;
using System.Collections.Generic;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Finds the live IC editor board carrying a thing id and builds one
/// <see cref="StationeersComputer"/> for it. Reads Unity's objects on the
/// main thread, so the computer it hands back already names one editor.
/// </summary>
public sealed class StationeersComputerRepository : IComputerRepository
{
    /// <summary>One IC editor board, its label, and the ids that reach it.</summary>
    private sealed class Board
    {
        public Board(ProgrammableChipMotherboard motherboard, string name, long thingId, long computerId)
        {
            Motherboard = motherboard;
            Name = name;
            ThingId = thingId;
            ComputerId = computerId;
        }

        public ProgrammableChipMotherboard Motherboard { get; }

        public string Name { get; }

        public long ThingId { get; }

        public long ComputerId { get; }
    }

    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersComputerRepository(IMainThread threads, Logger log)
    {
        _threads = threads;
        _log = log;
    }

    public IComputer Find(long computerId)
    {
        try
        {
            return _threads.RunSync(() => FindInner(computerId));
        }
        catch (Exception ex) when (!(ex is ThingNotFound))
        {
            // A game fault, not a missed id: record it, then let the caller report it.
            _log.Info(ex.ToString());
            throw;
        }
    }

    private IComputer FindInner(long computerId)
    {
        var boards = All();
        foreach (var board in boards)
        {
            if (board.ThingId == computerId || (board.ComputerId != 0 && board.ComputerId == computerId))
            {
                return new StationeersComputer(board.Motherboard, board.Name, _threads, _log);
            }
        }

        throw new ThingNotFound("Computer", computerId, null, Described(boards));
    }

    /// <summary>The boards in the world, each labelled and numbered for a missed lookup.</summary>
    private static IReadOnlyList<Board> All()
    {
        var boards = new List<Board>();
        foreach (var motherboard in UnityEngine.Object.FindObjectsOfType<ProgrammableChipMotherboard>())
        {
            if (motherboard == null)
            {
                continue;
            }

            long thingId = 0;
            try
            {
                thingId = motherboard.ReferenceId;
            }
            catch (Exception)
            {
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

            boards.Add(new Board(motherboard, computer ?? own ?? "unnamed editor", thingId, computerId));
        }

        return boards;
    }

    /// <summary>Boards as <c>label #id</c>, so a missed lookup shows what to retry.</summary>
    private static string Described(IReadOnlyList<Board> boards)
    {
        var listed = new List<string>();
        foreach (var board in boards)
        {
            var entry = board.ThingId != 0 ? $"{board.Name} #{board.ThingId}" : board.Name;
            if (!listed.Contains(entry))
            {
                listed.Add(entry);
            }
        }

        return listed.Count == 0 ? "none" : string.Join(", ", listed.ToArray());
    }
}
