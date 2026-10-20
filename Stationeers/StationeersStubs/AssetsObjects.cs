// Minimal Assets.Scripts.Objects fakes: Thing hierarchy only.
// Real chain: Thing : MonoBehaviour, Structure : Thing,
// SmallGrid : Structure, SmallSingleGrid : SmallGrid,
// DynamicThing : Thing, Item : DynamicThing, Entity : DynamicThing.
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Objects
{
    public class Thing : MonoBehaviour
    {
        public string DisplayName => string.Empty;
        public string PrefabName => string.Empty;
        public long ReferenceId => 0;
        public Vector3 CenterPosition => Vector3.zero;
        public Vector3 GridPosition => Vector3.zero;
        public bool isActiveAndEnabled => false;
        public Reagents.ReagentMixture ReagentMixture => null;
    }

    public class Structure : Thing
    {
        public List<BuildState> BuildStates => new List<BuildState>();
        public int CurrentBuildStateIndex => 0;
        public bool IsStructureCompleted => true;
    }

    public class SmallGrid : Structure { }

    public class SmallSingleGrid : SmallGrid { }

    public class DynamicThing : Thing
    {
        public List<Slot> Slots => new List<Slot>();
        public object RootParentHuman => null;
        public GridSystem.Room Room => null;
    }

    public class Entity : DynamicThing { }

    public class DraggableThing : DynamicThing { }

    public class PortableAtmospherics : DraggableThing
    {
        public Assets.Scripts.Atmospherics.Atmosphere InternalAtmosphere => null;
        public object ContentType => null;
    }

    public class PortablesConnector : Structure
    {
        public Slot TankSlot => null;
    }

    public class Item : DynamicThing
    {
        public string CustomName => string.Empty;
        public int Quantity => 0;
    }

    public class BuildState
    {
        public ToolUse Tool => null;
        public int Quantity => 0;
        public string Details => string.Empty;
        public string Description => string.Empty;
    }

    public class Slot
    {
        public string DisplayName => string.Empty;
        public object Type => null;
        public DynamicThing Get() => null;
        public T Get<T>() where T : Thing => null;
        public DynamicThing Occupant => null;
    }
}

namespace Assets.Scripts.Objects.Pipes
{
    using Assets.Scripts.Networks;

    public interface ILogicable
    {
        int TotalSlots { get; }
        Thing GetAsThing { get; }
        Slot GetSlot(int index);
        bool CanLogicRead(Motherboards.LogicType type);
        bool CanLogicWrite(Motherboards.LogicType type);
        double GetLogicValue(Motherboards.LogicType type);
    }

    public class Device : SmallGrid
    {
        public Cable[] PowerCables => System.Array.Empty<Cable>();
        public Cable[] DataCables => System.Array.Empty<Cable>();
        public List<PipeNetwork> ConnectedPipeNetworks => new List<PipeNetwork>();
        public List<ChuteNetwork> ConnectedChuteNetworks => new List<ChuteNetwork>();
        public List<CableNetwork> ConnectedCableNetworks => new List<CableNetwork>();
        public bool HasOnOffState => false;
        public bool OnOff => false;
        public float UsedPower => 0;
    }

    public class DeviceAtmospherics : Device { }

    public class DeviceInputOutput : DeviceAtmospherics
    {
        public PipeNetwork InputNetwork => null;
        public PipeNetwork InputNetwork2 => null;
        public PipeNetwork OutputNetwork => null;
        public PipeNetwork OutputNetwork2 => null;
    }

    public class Pipe : SmallSingleGrid
    {
        public PipeNetwork PipeNetwork => null;
    }

    public class Chute : SmallSingleGrid
    {
        public ChuteNetwork ChuteNetwork => null;
    }

    public class Cable : SmallSingleGrid
    {
        public CableNetwork CableNetwork => null;
    }
}

namespace Assets.Scripts.Networks
{
    public class PipeNetworkCollection : List<PipeNetwork>
    {
        public System.Collections.Generic.IEnumerable<PipeNetwork> Active() => this;
    }

    public class PipeNetwork
    {
        public static PipeNetworkCollection AllPipeNetworks => new PipeNetworkCollection();
        public long ReferenceId => 0;
        public Assets.Scripts.Atmospherics.Atmosphere Atmosphere => null;
        public object NetworkContentType => null;
        public List<Assets.Scripts.Objects.SmallGrid> StructureList => new List<Assets.Scripts.Objects.SmallGrid>();
        public List<Assets.Scripts.Objects.Pipes.Device> DeviceList => new List<Assets.Scripts.Objects.Pipes.Device>();
    }

    public class CableNetwork
    {
        public long ReferenceId => 0;
    }

    public class ChuteNetwork
    {
        public long ReferenceId => 0;
    }
}
