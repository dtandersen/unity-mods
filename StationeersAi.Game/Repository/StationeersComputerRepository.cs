using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Repository;

/// <summary>
/// Builds the live computer behind the core <see cref="IComputerRepository"/>
/// from one motherboard. <see cref="Find"/> reads the boards on Unity's main
/// thread, so the computer it hands back already names one editor.
/// </summary>
public sealed class StationeersComputerRepository : IComputerRepository
{
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
        var boards = ComputerBoards.Match(computer);
        if (boards.Count == 0)
        {
            return new ComputerLookup(null, $"Computer '{computer}' not found. Available: {ComputerBoards.Available()}");
        }

        if (boards.Count > 1)
        {
            return new ComputerLookup(null, $"Several computers match '{computer}': {ComputerBoards.Names(boards)}");
        }

        ProgrammableChipMotherboard motherboard = boards[0].Motherboard;
        return new ComputerLookup(new StationeersComputer(motherboard, boards[0].Name, _threads, _log), null);
    }
}
