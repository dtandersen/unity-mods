// Minimal LocoSim fakes: only what DriverAssist compiles against.
using System.Collections.Generic;
using UnityEngine;

namespace LocoSim.Implementations
{
    public class SimController : MonoBehaviour
    {
        public BaseControlsOverrider controlsOverrider;
        public SimulationFlow simFlow;
    }

    public class BaseControlsOverrider
    {
        public ControlValue Throttle = new ControlValue();
        public ControlValue Brake = new ControlValue();
        public ControlValue IndependentBrake = new ControlValue();
        public ControlValue Reverser = new ControlValue();
    }

    public class ControlValue
    {
        public float Value => 0f;
        public void Set(float value) { }
    }

    public class SimulationFlow
    {
        public List<SimComponent> OrderedSimComps = new List<SimComponent>();

        public bool TryGetPort(string id, out Port port)
        {
            port = new Port();
            return false;
        }
    }

    public class Port
    {
        public string id;
        public float Value => 0f;
    }

    public class SimComponent
    {
        public List<Port> GetAllPorts() => new List<Port>();
    }
}
