using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI;
using StationeersAi.Entity;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Repository;

/// <summary>
/// Reads the in-game encyclopedia. Asks the provider for the current pages
/// on every read and maps them on Unity's main thread, so it holds no
/// snapshot and never goes stale.
/// </summary>
public sealed class StationeersStationpediaRepository : IStationpediaRepository
{
    private readonly IStationpediaProvider _pages;
    private readonly IMainThread _executor;
    private readonly Logger _log;

    public StationeersStationpediaRepository(IStationpediaProvider pages, IMainThread executor, Logger log)
    {
        _pages = pages;
        _executor = executor;
        _log = log;
    }

    public IReadOnlyList<StationpediaEntry> GetAll()
    {
        try
        {
            return _executor.RunSync(ReadAll);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new StationpediaEntry[0];
        }
    }

    public StationpediaEntry? GetByKey(string? key)
    {
        return GetAll().FirstOrDefault(entry => entry.Key == key);
    }

    private IReadOnlyList<StationpediaEntry> ReadAll()
    {
        var entries = new List<StationpediaEntry>();
        var pages = _pages.GetPages();
        if (pages == null)
        {
            return entries;
        }

        foreach (var page in pages)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Key))
            {
                continue;
            }

            entries.Add(new StationpediaEntry(
                Key: page.Key,
                Title: string.IsNullOrWhiteSpace(page.Title) ? page.Key : page.Title,
                Description: page.Description,
                Text: string.IsNullOrWhiteSpace(page.Text) ? page.ConstructWithText : page.Text));
        }

        return entries;
    }
}
