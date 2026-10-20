// Minimal Assets.Scripts.Atmospherics fakes.
using System.Collections.Generic;

namespace Assets.Scripts.Atmospherics
{
    public struct TemperatureKelvin
    {
        public double ToCelsius() => 0;
        public double ToDouble() => 0;
    }

    public struct PressurekPa
    {
        public double ToFloat() => 0;
        public double ToDouble() => 0;
    }

    public struct VolumeLitres
    {
        public double ToFloat() => 0;
        public double ToDouble() => 0;
    }

    public struct Moles
    {
        public double Quantity => 0;
        public double ToFloat() => 0;
        public double ToDouble() => 0;
    }

    public class GasSlot
    {
        public Moles Quantity => new Moles();
    }

    public class GasMixture
    {
        public GasSlot Oxygen => null;
        public GasSlot Nitrogen => null;
        public GasSlot CarbonDioxide => null;
        public GasSlot Hydrogen => null;
        public GasSlot Methane => null;
        public GasSlot Pollutant => null;
        public GasSlot Water => null;
        public GasSlot Helium => null;
        public GasSlot NitrousOxide => null;
        public GasSlot LiquidNitrogen => null;
        public GasSlot LiquidOxygen => null;
        public GasSlot LiquidHydrogen => null;
        public GasSlot LiquidMethane => null;
        public GasSlot LiquidCarbonDioxide => null;
        public GasSlot LiquidPollutant => null;
        public GasSlot LiquidNitrousOxide => null;
        public GasSlot LiquidWater => null;
        public GasSlot LiquidVolatiles => null;
        public GasSlot Steam => null;
        public GasSlot PollutedWater => null;
        public GasSlot Hydrazine => null;
        public GasSlot LiquidHydrazine => null;
        public GasSlot LiquidAlcohol => null;
        public GasSlot LiquidSodiumChloride => null;
        public GasSlot Silanol => null;
        public GasSlot LiquidSilanol => null;
        public GasSlot HydrochloricAcid => null;
        public GasSlot LiquidHydrochloricAcid => null;
        public GasSlot Ozone => null;
        public GasSlot LiquidOzone => null;
        public double Temperature => 0;
    }

    public class Atmosphere
    {
        public TemperatureKelvin Temperature => new TemperatureKelvin();
        public PressurekPa Pressure => new PressurekPa();
        public PressurekPa PressureGassesAndLiquids => new PressurekPa();
        public VolumeLitres Volume => new VolumeLitres();
        public Moles TotalMoles => new Moles();
        public GasMixture GasMixture => null;
    }

    public static class Chemistry
    {
        public enum GasType { Oxygen, Nitrogen, CarbonDioxide, Volatiles, Pollutant, Water, Hydrogen, Helium, Methane, NitrousOxide, LiquidOxygen, LiquidNitrogen, LiquidCarbonDioxide, LiquidVolatiles, LiquidPollutant, LiquidWater, LiquidHydrogen, LiquidMethane, LiquidNitrousOxide }

        public static class Temperature
        {
            public static double ZeroDegrees => 273.15;
            public static double TwentyDegrees => 293.15;
        }

        public static string GetSymbol(GasType gas) => string.Empty;
    }

    public class Mole
    {
        public Mole(Chemistry.GasType type, double count, double energy) { }
        public double AutoIgnitionTemperature => 0;
        public double AutoIgnitionOffset => 0;
        public static AtmosphereHelper.MatterState MatterState(Chemistry.GasType gas) => 0;
        public static double SpecificHeat(Chemistry.GasType gas) => 0;
        public static double BoilingPoint(Chemistry.GasType gas) => 0;
    }

    public static class AtmosphereHelper
    {
        public enum MatterState { Gas, Liquid, Solid }
    }

    public static class IdealGas
    {
        public static double Energy(double temp, double heat, double moles) => 0;
    }

    public class CombustionOutput
    {
        public Chemistry.GasType GasType => 0;
        public object Quantity => null;
    }

    public class CombustionResult
    {
        public double FuelMoleCount => 0;
        public double OxidiserMoleCount => 0;
        public List<CombustionOutput> Outputs => new List<CombustionOutput>();
        public GasMixture RunCombustion(Mole fuel, Mole oxidiser, double ratio, out object a, out object b, out object c)
        {
            a = null; b = null; c = null;
            return null;
        }
    }

    public static class Combustion
    {
        public static bool TryGetResult(Chemistry.GasType fuel, Chemistry.GasType ox, out CombustionResult result)
        {
            result = null;
            return false;
        }
    }

    public static class DoubleExtensions
    {
        public static double ToCelsius(this double kelvin) => 0;
        public static double ToFloat(this double v) => 0;
        public static double ToDouble(this double v) => v;
        public static double ToDouble(this object v) => 0;
    }
}
