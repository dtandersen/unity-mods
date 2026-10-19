using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.UI;
using StationeersAi.Entity;
using StationeersAi.Formatting;
using StationpediaUseEntity = StationeersAi.Entity.StationpediaUse;
using StationpediaMakeEntity = StationeersAi.Entity.StationpediaMake;
using StationpediaCombustionEntity = StationeersAi.Entity.StationpediaCombustion;
using StationpediaCombustionOutputEntity = StationeersAi.Entity.StationpediaCombustionOutput;
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
                HowToBuild: SafeHowToBuild(page),
                Combustion: SafeCombustion(page),
                BuildSteps: SafeBuildSteps(page)));
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

    /// <summary>
    /// How to place the page's subject stage by stage: one line per build
    /// state from the game's own Stationpedia data, rich text stripped to
    /// plain words. A page that fails to report keeps no steps rather than
    /// dropping the whole encyclopedia.
    /// </summary>
    private static IReadOnlyList<string> SafeBuildSteps(StationpediaPage page)
    {
        try
        {
            var steps = new List<string>();
            foreach (var insert in page.BuildStates)
            {
                if (insert == null)
                {
                    continue;
                }

                var details = UnityRichText.Strip(insert.Details);
                var description = UnityRichText.Strip(insert.Description);
                var line = (details + " " + description).Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                steps.Add(line);
            }

            return steps;
        }
        catch (System.Exception)
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// How the page's subject burns: fuel and oxidiser with mole counts,
    /// outputs, auto ignition and output heat. Values come from the game's
    /// combustion tables on every read, using the same maths as the
    /// Stationpedia UI, so no numbers live in this repo. A page that fails
    /// to report keeps no burns rather than dropping the whole encyclopedia.
    /// </summary>
    private static IReadOnlyList<StationpediaCombustionEntity> SafeCombustion(StationpediaPage page)
    {
        try
        {
            var burns = new List<StationpediaCombustionEntity>();
            foreach (var insert in page.CombustionInserts)
            {
                if (insert == null)
                {
                    continue;
                }

                if (!Combustion.TryGetResult(insert.FuelType, insert.OxidiserType, out var result))
                {
                    continue;
                }

                var fuelTemp = Chemistry.Temperature.TwentyDegrees;
                if (Mole.MatterState(insert.FuelType) == AtmosphereHelper.MatterState.Liquid)
                {
                    fuelTemp = Mole.BoilingPoint(insert.FuelType);
                }

                var oxidiserTemp = Chemistry.Temperature.TwentyDegrees;
                if (Mole.MatterState(insert.OxidiserType) == AtmosphereHelper.MatterState.Liquid)
                {
                    oxidiserTemp = Mole.BoilingPoint(insert.OxidiserType);
                }

                var fuel = new Mole(
                    insert.FuelType,
                    result.FuelMoleCount,
                    IdealGas.Energy(fuelTemp, Mole.SpecificHeat(insert.FuelType), result.FuelMoleCount));
                var oxidiser = new Mole(
                    insert.OxidiserType,
                    result.OxidiserMoleCount,
                    IdealGas.Energy(oxidiserTemp, Mole.SpecificHeat(insert.OxidiserType), result.OxidiserMoleCount));
                var mixture = result.RunCombustion(fuel, oxidiser, 1.0, out _, out _, out _);

                var autoC = (fuel.AutoIgnitionTemperature + oxidiser.AutoIgnitionOffset - Chemistry.Temperature.ZeroDegrees).ToDouble();
                var outC = (mixture.Temperature - Chemistry.Temperature.ZeroDegrees).ToDouble();
                var outputs = result.Outputs
                    .Select(o => new StationpediaCombustionOutputEntity(Symbol(o.GasType), o.Quantity.ToDouble()))
                    .ToArray();
                burns.Add(new StationpediaCombustionEntity(
                    Symbol(insert.FuelType),
                    Symbol(insert.OxidiserType),
                    result.FuelMoleCount.ToDouble(),
                    result.OxidiserMoleCount.ToDouble(),
                    outputs,
                    autoC,
                    Math.Round(outC)));
            }

            return burns;
        }
        catch (System.Exception)
        {
            return new StationpediaCombustionEntity[0];
        }
    }

    private static string Symbol(Chemistry.GasType gas)
    {
        var symbol = Chemistry.GetSymbol(gas);
        return string.IsNullOrWhiteSpace(symbol) ? gas.ToString() : symbol;
    }
}
