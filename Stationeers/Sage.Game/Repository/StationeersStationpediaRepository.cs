using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Sage.Entity;
using Sage.Formatting;
using StationpediaUseEntity = Sage.Entity.StationpediaUse;
using StationpediaMakeEntity = Sage.Entity.StationpediaMake;
using StationpediaCombustionEntity = Sage.Entity.StationpediaCombustion;
using StationpediaCombustionOutputEntity = Sage.Entity.StationpediaCombustionOutput;
using Sage.Game;
using Logger = Sage.Core.Logger;

namespace Sage.Repository;

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
                UsedIn: SafeUsedIn(page, SubjectFuelEps(page), TickSeconds()),
                HowToBuild: SafeHowToBuild(page),
                Combustion: SafeCombustion(page),
                BuildSteps: SafeBuildSteps(page),
                PowerGenerationW: SubjectPowerGenerationW(page),
                PowerStorageJ: SubjectPowerStorageJ(page),
                PowerDrawW: SubjectPowerDrawW(page),
                FuelEnergyPerSecondW: SubjectFuelEps(page),
                TickSeconds: TickSeconds(),
                Logic: SafeLogic(page),
                Connections: SafeConnections(page)));
        }

        return entries;
    }

    /// <summary>
    /// What consumes the page's subject, with the energy detail on fuels
    /// (e.g. <c>100 kW Energy</c>). Rich text is stripped to plain words.
    /// When the subject is a solid fuel and the consumer is a power
    /// generator, the numeric fuel fields carry the game's own maths:
    /// burn ticks = round(energy per second / generator output) * 2,
    /// total joules = ticks * output, burn seconds = ticks * tick length.
    /// A page that fails to report keeps no uses rather than dropping the
    /// whole encyclopedia.
    /// </summary>
    private static IReadOnlyList<StationpediaUseEntity> SafeUsedIn(StationpediaPage page, double? subjectFuelEps, double? tickSeconds)
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

                double? generatorPower = null;
                int? burnTicks = null;
                double? totalJoules = null;
                double? burnSeconds = null;
                if (subjectFuelEps.HasValue && tickSeconds.HasValue)
                {
                    generatorPower = GeneratorPowerW(insert.PrefabHash);
                    if (generatorPower.HasValue && generatorPower.Value > 0)
                    {
                        burnTicks = (int)Math.Round(subjectFuelEps.Value / generatorPower.Value) * 2;
                        totalJoules = burnTicks.Value * generatorPower.Value;
                        burnSeconds = burnTicks.Value * tickSeconds.Value;
                    }
                }

                uses.Add(new StationpediaUseEntity(
                    insert.NameOfThing,
                    string.IsNullOrWhiteSpace(detail) ? null : detail,
                    string.IsNullOrWhiteSpace(insert.PageLink) ? null : insert.PageLink,
                    subjectFuelEps,
                    generatorPower,
                    burnTicks,
                    totalJoules,
                    burnSeconds,
                    tickSeconds));
            }

            return uses;
        }
        catch (System.Exception)
        {
            return new StationpediaUseEntity[0];
        }
    }

    private static double? TickSeconds()
    {
        try
        {
            return GameManager.GameTickSpeedSeconds;
        }
        catch (System.Exception)
        {
            return 0.5;
        }
    }

    private static Thing? SubjectPrefab(StationpediaPage page)
    {
        try
        {
            if (page == null || page.PrefabHash == 0)
            {
                return null;
            }

            return Prefab.Find(page.PrefabHash);
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static double? SubjectFuelEps(StationpediaPage page)
    {
        try
        {
            var subject = SubjectPrefab(page);
            if (subject is ISolidFuel fuel)
            {
                return fuel.GetEnergyPerSecond();
            }

            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static double? GeneratorPowerW(int prefabHash)
    {
        try
        {
            if (prefabHash == 0)
            {
                return null;
            }

            var prefab = Prefab.Find(prefabHash);
            if (prefab is PowerGeneratorSlot slot)
            {
                return slot.PowerGenerated;
            }

            if (prefab is global::Objects.IPowerGenerator generator)
            {
                return generator.GetMaxPowerGenerated();
            }

            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static double? SubjectPowerGenerationW(StationpediaPage page)
    {
        try
        {
            var subject = SubjectPrefab(page);
            if (subject is PowerGeneratorSlot slot)
            {
                return slot.PowerGenerated;
            }

            if (subject is global::Objects.IPowerGenerator generator)
            {
                return generator.GetMaxPowerGenerated();
            }

            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static double? SubjectPowerStorageJ(StationpediaPage page)
    {
        try
        {
            var subject = SubjectPrefab(page);
            if (subject is IChargable chargable)
            {
                return chargable.GetPowerMaximum();
            }

            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static double? SubjectPowerDrawW(StationpediaPage page)
    {
        try
        {
            var subject = SubjectPrefab(page);
            if (subject is Device device && device.UsedPower > 0)
            {
                return device.UsedPower;
            }

            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Logic variables the page's subject reads or writes, taken from the
    /// game's own Stationpedia data. Names are plain words (e.g. Power,
    /// On, ForceWrite) and access is Read, Write, or Read Write. A page
    /// that fails to report keeps no logic rather than dropping the whole
    /// encyclopedia.
    /// </summary>
    private static IReadOnlyList<Sage.Entity.StationpediaLogic> SafeLogic(StationpediaPage page)
    {
        try
        {
            var lines = new List<Sage.Entity.StationpediaLogic>();
            foreach (var insert in page.LogicInsert)
            {
                if (insert == null || string.IsNullOrWhiteSpace(insert.LogicName))
                {
                    continue;
                }

                var name = UnityRichText.Strip(insert.LogicName).Trim();
                var access = UnityRichText.Strip(insert.LogicAccessTypes).Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                lines.Add(new Sage.Entity.StationpediaLogic(
                    name,
                    string.IsNullOrWhiteSpace(access) ? "Read" : access));
            }

            return lines;
        }
        catch (System.Exception)
        {
            return new Sage.Entity.StationpediaLogic[0];
        }
    }

    /// <summary>
    /// Data and cable ports of the page's subject, taken from the game's
    /// own Stationpedia data. Names are plain words (e.g. Data Input,
    /// Data Output) with the port index on the prefab. A page that fails
    /// to report keeps no ports rather than dropping the whole
    /// encyclopedia.
    /// </summary>
    private static IReadOnlyList<Sage.Entity.StationpediaConnection> SafeConnections(StationpediaPage page)
    {
        try
        {
            var ports = new List<Sage.Entity.StationpediaConnection>();
            foreach (var insert in page.ConnectionInsert)
            {
                if (insert == null || string.IsNullOrWhiteSpace(insert.LogicName))
                {
                    continue;
                }

                var name = UnityRichText.Strip(insert.LogicName).Trim();
                var index = UnityRichText.Strip(insert.LogicAccessTypes).Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                ports.Add(new Sage.Entity.StationpediaConnection(
                    name,
                    string.IsNullOrWhiteSpace(index) ? "0" : index));
            }

            return ports;
        }
        catch (System.Exception)
        {
            return new Sage.Entity.StationpediaConnection[0];
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
