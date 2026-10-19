using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using StationpediaUseEntity = StationeersAi.Entity.StationpediaUse;
using StationpediaMakeEntity = StationeersAi.Entity.StationpediaMake;
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
                Text: string.IsNullOrWhiteSpace(page.Text) ? page.ConstructWithText : page.Text,
                UsedIn: SafeUsedIn(page),
                HowToBuild: SafeHowToBuild(page)));
        }

        return entries;
    }

    /// <summary>
    /// What consumes the page's subject, with the energy detail on fuels
    /// (e.g. <c>100 kW Energy</c>). Rich text is stripped to plain words.
    /// A page that fails to report keeps no uses rather than dropping the
    /// whole encyclopedia.
    /// </summary>
    private static IReadOnlyList<StationpediaUseEntity> SafeUsedIn(StationpediaPage page)
    {
        try
        {
            var uses = new List<StationpediaUseEntity>();
            foreach (var insert in page.UsedIn)
            {
                if (insert == null || string.IsNullOrWhiteSpace(insert.NameOfThing))
                {
                    continue;
                }

                string? detail = null;
                if (insert is StationCategoryInsertWithSubtext fueled &&
                    (!string.IsNullOrWhiteSpace(fueled.SubText) || !string.IsNullOrWhiteSpace(fueled.SubValue)))
                {
                    detail = (UnityRichText.Strip(fueled.SubText) + " " + UnityRichText.Strip(fueled.SubValue)).Trim();
                }

                uses.Add(new StationpediaUseEntity(
                    insert.NameOfThing,
                    string.IsNullOrWhiteSpace(detail) ? null : detail,
                    string.IsNullOrWhiteSpace(insert.PageLink) ? null : insert.PageLink));
            }

            return uses;
        }
        catch (System.Exception)
        {
            return new StationpediaUseEntity[0];
        }
    }

    /// <summary>
    /// How to manufacture the page's subject: the station, its tier, and
    /// the requirements text with smelt conditions. Rich text is stripped
    /// to plain words. A page that fails to report keeps no makes rather
    /// than dropping the whole encyclopedia.
    /// </summary>
    private static IReadOnlyList<StationpediaMakeEntity> SafeHowToBuild(StationpediaPage page)
    {
        try
        {
            var makes = new List<StationpediaMakeEntity>();
            foreach (var insert in page.HowToBuild)
            {
                if (insert == null || string.IsNullOrWhiteSpace(insert.PrinterName))
                {
                    continue;
                }

                var detail = UnityRichText.Strip(insert.Description);
                makes.Add(new StationpediaMakeEntity(
                    Stationpedia.Trim(insert.PrinterName),
                    string.IsNullOrWhiteSpace(insert.TierName) ? null : insert.TierName,
                    string.IsNullOrWhiteSpace(detail) ? null : detail,
                    string.IsNullOrWhiteSpace(insert.PageLink) ? null : insert.PageLink));
            }

            return makes;
        }
        catch (System.Exception)
        {
            return new StationpediaMakeEntity[0];
        }
    }
}
