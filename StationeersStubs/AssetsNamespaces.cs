// Minimal fakes for remaining Stationeers namespaces.
// Only members the mod touches; bodies never run (CI compile only).
// Namespaces mirror the real game (see ../stationeers).
using System.Collections.Generic;
using Assets.Scripts.Objects;

namespace Assets.Scripts.Networking
{
    public enum NetworkChannel { GeneralTraffic }

    public class ChatMessage : UnityEngine.MonoBehaviour
    {
        public string ChatText { get; set; }
        public string DisplayName { get; set; }
        public int HumanId { get; set; }
        public void PrintToConsole() { }
    }

    public static class NetworkManager
    {
        public static bool IsClient => false;
        public static bool IsServer => false;
        public static int TotalPlayersInGame => 0;
    }

    public static class NetworkClient
    {
        public static void SendToServer(ChatMessage msg, NetworkChannel channel) { }
    }

    public static class NetworkServer
    {
        public static void SendToClients(ChatMessage msg, NetworkChannel channel, long id) { }
    }
}

namespace Assets.Scripts
{
    public static class GameManager
    {
        public static void LeaveGame() { }
        public static double GameTickSpeedSeconds => 0;
        public static GridSystem.GameState GameState => GridSystem.GameState.Running;
    }

    public class ConsoleWindow
    {
        public static void Draw() { }
    }

    public struct TimeLength
    {
        public int Days => 0;
        public int Hours => 0;
        public int Minutes => 0;
        public int Seconds => 0;
        public double TotalSeconds => 0;
    }

    public class RoomController
    {
        public static RoomController World => null;
        public static GridSystem.Room Get(long id) => null;
        public GridSystem.Room GetRoom(UnityEngine.Vector3 pos) => null;
        public GridSystem.Room GetRoom(GridSystem.Grid3 grid) => null;
    }
}

namespace Assets.Scripts.GridSystem
{
    public enum GameState : byte { None, Joining, Waiting, Running, Loading, Paused }

    public struct Grid3 { }

    public class Room
    {
        public long RoomId;
        public object RoomType => null;
        public Atmospherics.TemperatureKelvin Temperature => new Atmospherics.TemperatureKelvin();
        public Atmospherics.PressurekPa Pressure => new Atmospherics.PressurekPa();
        public Atmospherics.VolumeLitres Volume => new Atmospherics.VolumeLitres();
        public Atmospherics.GasMixture GasMixture => null;
    }
}

namespace Assets.Scripts.Objects
{
    public static class Prefab
    {
        public static List<Thing> AllPrefabs => new List<Thing>();
        public static Thing Find(int hash) => null;
        public static Thing Find(long hash) => null;
    }

    public class ToolUse
    {
        public Item ToolEntry => null;
        public int EntryQuantity => 0;
        public Item ToolEntry2 => null;
        public int EntryQuantity2 => 0;
    }
}

// Global (no namespace): WorldManager, WorldSetting, OrbitalSimulation, etc.
public class WorldManager
{
    public static string CurrentWorldName => string.Empty;
    public static string CurrentWorldId => string.Empty;
    public static float WorldGravity => 0;
    public static uint DaysPast => 0;
    public static bool HasLava => false;
    public static bool IsCreative() => false;
    public static event System.Action OnWorldStarted;
}

public class WorldSetting
{
    public static WorldSetting Current => null;
    public WorldSettingData Data => null;
}

public class WorldSettingData
{
    public GlobalAtmosphereData GlobalAtmosphereData => null;
}

public class GlobalAtmosphereData
{
    public GlobalGasMixData GlobalGasMixData => null;
}

public class GlobalGasMixData
{
    public List<GlobalMoleData> GlobalMoleDatas => new List<GlobalMoleData>();
}

public class GlobalMoleData
{
    public float Quantity => 0;
    public object Type => null;
}

public class Orbit
{
    public float Eccentricity => 0;
    public double SemiMajorAxis => 0;
    public float Inclination => 0;
}

public class RotatingCelestialBody
{
    public Assets.Scripts.TimeLength GetSiderealYearLength() => new Assets.Scripts.TimeLength();
    public double DaysPerPlanetYear => 0;
    public Orbit Orbit => null;
    public bool TidallyLocked => false;
}

public static class OrbitalSimulation
{
    public static RotatingCelestialBody GetPlayerBody() => null;
    public static int GetDayLengthSeconds() => 0;
    public static float SolarIrradiance => 0;
    public static float EarthSolarRatio => 0;
    public static float TimeOfDay => 0;
    public static bool IsEclipse => false;
}

public static class PlanetaryAtmosphereSimulation
{
    public static Assets.Scripts.Atmospherics.TemperatureKelvin AggregateTemperature => new Assets.Scripts.Atmospherics.TemperatureKelvin();
    public static Assets.Scripts.Atmospherics.PressurekPa GlobalPressure => new Assets.Scripts.Atmospherics.PressurekPa();
}

namespace Assets.Scripts.Objects.Electrical
{
    public interface ICircuitHolder
    {
        List<Motherboards.LogicBinding> GetLogicBindings();
        Pipes.ILogicable GetLogicableFromIndex(int index);
    }

    public class CircuitHousing : Pipes.Device, ICircuitHolder
    {
        public Slot _ProgrammableChipSlot => null;
        public Pipes.ILogicable[] Devices => new Pipes.ILogicable[0];
        public string GetSourceCode() => string.Empty;
        public void SetSourceCode(string code) { }
        public List<Motherboards.LogicBinding> GetLogicBindings() => new List<Motherboards.LogicBinding>();
        public Pipes.ILogicable GetLogicableFromIndex(int index) => null;
    }

    public enum ScriptCommand { Nop }

    public class ProgrammableChip : Item
    {
        public string CompileErrorType => string.Empty;
        public int CompileErrorLine => 0;
        public bool CompilationError => false;
        public string ErrorTypeString => string.Empty;
        public string ErrorLineNumberString => string.Empty;
        public string GetSourceCode() => string.Empty;
        public void SetSourceCode(string code) { }
        public void SetSourceCode(string code, ICircuitHolder parent) { }
        public void SendUpdate() { }
        public static string StripColorTags(string s) => s;
        public static string GetCommandExample(ScriptCommand cmd) => string.Empty;
        public static string GetCommandDescription(ScriptCommand cmd) => string.Empty;
    }

    public class PowerGeneratorSlot : Pipes.Device
    {
        public double PowerGenerated => 0;
    }
}

namespace Assets.Scripts.Objects.Motherboards
{
    public enum LogicType : ushort { None, Setting }

    public static class LogicBase
    {
        public static bool IsDeprecated(Electrical.ScriptCommand cmd) => false;
        public static bool IsDeprecated(LogicType type) => false;
    }

    public class LogicBinding
    {
        public LogicType LogicType => 0;
        public string Header => string.Empty;
    }

    public class ProgrammableChipMotherboard : Pipes.Device, Electrical.ICircuitHolder
    {
        public Slot _ProgrammableChipSlot => null;
        public MotherboardComputer ParentComputer => null;
        public string GetSourceCode() => string.Empty;
        public void SetSourceCode(string code) { }
        public void SendUpdate() { }
        public void Export() { }
        public void Import() { }
        public List<LogicBinding> GetLogicBindings() => new List<LogicBinding>();
        public Pipes.ILogicable GetLogicableFromIndex(int index) => null;
    }

    public class MotherboardComputer
    {
        public Assets.Scripts.Objects.Thing AsThing() => null;
    }
}

namespace Assets.Scripts.Objects.Entities
{
    public class Human : Entity
    {
        public static Human LocalHuman => null;
        public string CustomName => string.Empty;
        public Slot LeftHandSlot => null;
        public Slot RightHandSlot => null;
        public Slot UniformSlot => null;
        public Slot SuitSlot => null;
        public Slot HelmetSlot => null;
        public Slot GlassesSlot => null;
        public Slot BackpackSlot => null;
        public Slot ToolbeltSlot => null;
        public Slot StomachSlot => null;
        public UnityEngine.Vector3 EntityForward => UnityEngine.Vector3.zero;
        public UnityEngine.Vector3 ThingTransformPosition => UnityEngine.Vector3.zero;
        public GridSystem.Room Room => null;
    }
}

namespace Assets.Scripts.Objects.Items
{
    public class RecipeReference
    {
        public int PrefabHash => 0;
        public DynamicThing DynamicThing => null;
        public Reagents.Recipe Recipe => null;
        public DynamicThing Creator => null;
    }

    public interface IQuantity
    {
        string GetQuantityText();
    }

    public interface ISolidFuel
    {
        double GetEnergyPerSecond();
    }

    public interface IChargable
    {
        double GetPowerMaximum();
    }

    public class ElectronicReader : DynamicThing { }
}

namespace Assets.Scripts.UI
{
    public class StationpediaPage
    {
        public string Key => string.Empty;
        public string Title => string.Empty;
        public string Description => string.Empty;
        public string Text => string.Empty;
        public int PrefabHash => 0;
        public List<BuildState> BuildStates => new List<BuildState>();
        public string ConstructWithText => string.Empty;
        public List<HowToBuildInsert> HowToBuild => new List<HowToBuildInsert>();
        public List<LogicInsert> LogicInsert => new List<LogicInsert>();
        public List<ConnectionInsert> ConnectionInsert => new List<ConnectionInsert>();
        public List<CombustionInsert> CombustionInserts => new List<CombustionInsert>();
        public List<StationCategoryInsertWithSubtext> UsedIn => new List<StationCategoryInsertWithSubtext>();
    }

    public class HowToBuildInsert
    {
        public string PrinterName => string.Empty;
        public string Description => string.Empty;
        public string TierName => string.Empty;
        public string PageLink => string.Empty;
    }

    public class LogicInsert
    {
        public string LogicName => string.Empty;
        public string LogicAccessTypes => string.Empty;
    }

    public class ConnectionInsert
    {
        public string LogicName => string.Empty;
        public string LogicAccessTypes => string.Empty;
    }

    public class CombustionInsert
    {
        public Atmospherics.Chemistry.GasType FuelType => 0;
        public Atmospherics.Chemistry.GasType OxidiserType => 0;
    }

    public class StationCategoryInsertWithSubtext
    {
        public string SubText => string.Empty;
        public string SubValue => string.Empty;
        public string NameOfThing => string.Empty;
        public string PageLink => string.Empty;
        public int PrefabHash => 0;
    }

    public static class Stationpedia
    {
        public static List<StationpediaPage> StationpediaPages => new List<StationpediaPage>();
        public static string Trim(string s) => s;
    }
}

namespace Reagents
{
    public class Recipe { }
    public class Reagent
    {
        public static List<Reagent> AllReagentsSorted => new List<Reagent>();
        public string DisplayName => string.Empty;
        public string TypeName => string.Empty;
    }
    public class ReagentMixture
    {
        public double Get(Reagent reagent) => 0;
    }
}

namespace Objects
{
    public interface IPowerGenerator
    {
        double GetMaxPowerGenerated();
    }
}
