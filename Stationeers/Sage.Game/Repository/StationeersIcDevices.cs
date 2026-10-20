using System;
using System.Collections.Generic;
using Sage.Core;
using Sage.Game;
using Logger = Sage.Core.Logger;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Sage.Entity;
using Sage.Repository;
using IComputer = Sage.Entity.IComputer;

namespace Sage.Repository;

/// <summary>
/// Builds the live IC devices behind <see cref="IIcDevices"/>. Each lookup
/// scans Unity's objects on the main thread, so the device it hands back
/// names one real thing: a computer's editor board, or a housing and the
/// chip it holds.
/// </summary>
public sealed class StationeersIcDevices : IIcDevices
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersIcDevices(IMainThread threads, Logger log)
    {
        _threads = threads;
        _log = log;
    }

    public IComputer FindComputer(long computerId)
    {
        return OnMainThread(() => ComputerInner(computerId));
    }

    public IHousing FindHousing(long thingId)
    {
        return OnMainThread(() => HousingInner(thingId));
    }

    /// <summary>
    /// Runs one lookup on the main thread. A missed id travels to the caller
    /// as it is; any other fault is recorded first, because the caller has no
    /// logger and can only report that the lookup failed.
    /// </summary>
    private T OnMainThread<T>(Func<T> lookup)
    {
        try
        {
            return _threads.RunSync(lookup);
        }
        catch (Exception ex) when (!(ex is ThingNotFound))
        {
            _log.Info(ex.ToString());
            throw;
        }
    }

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

    private IComputer ComputerInner(long computerId)
    {
        var boards = AllBoards();
        foreach (var board in boards)
        {
            if (board.ThingId == computerId || (board.ComputerId != 0 && board.ComputerId == computerId))
            {
                return new StationeersComputer(board.Motherboard, board.Name, _threads, _log);
            }
        }

        throw new ThingNotFound("Computer", computerId, null, BoardsAsText(boards));
    }

    /// <summary>The IC editor boards in the world, each labelled and numbered for a missed lookup.</summary>
    private static IReadOnlyList<Board> AllBoards()
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

    private static string BoardsAsText(IReadOnlyList<Board> boards)
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

    private IHousing HousingInner(long thingId)
    {
        var housings = AllHousings();
        foreach (var housing in housings)
        {
            long id;
            try
            {
                id = housing.ReferenceId;
            }
            catch (Exception)
            {
                continue;
            }

            if (id == thingId)
            {
                return new StationeersHousing(housing, _threads, _log);
            }
        }

        throw new ThingNotFound("Housing", thingId, null, HousingsAsText(housings));
    }

    private static IReadOnlyList<CircuitHousing> AllHousings()
    {
        var housings = new List<CircuitHousing>();
        foreach (var housing in UnityEngine.Object.FindObjectsOfType<CircuitHousing>())
        {
            if (housing != null)
            {
                housings.Add(housing);
            }
        }

        return housings;
    }

    private static string HousingsAsText(IReadOnlyList<CircuitHousing> housings)
    {
        var listed = new List<string>();
        foreach (var housing in housings)
        {
            string entry;
            try
            {
                entry = $"{housing.DisplayName} #{housing.ReferenceId}";
            }
            catch (Exception)
            {
                continue;
            }

            if (!listed.Contains(entry))
            {
                listed.Add(entry);
            }
        }

        return listed.Count == 0 ? "none" : string.Join(", ", listed.ToArray());
    }
}
