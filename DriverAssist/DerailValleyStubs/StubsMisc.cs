// Minimal DV.Simulation.Brake / DV.HUD / DV.Wheels / DV.Utils / I2.Loc fakes.
using UnityEngine;

namespace DV.Simulation.Brake
{
    public class BrakeSystem : MonoBehaviour
    {
        public float brakeCylinderPressure => 0f;

        public void ReleaseBrakeCylinderPressure() { }
    }
}

namespace DV.HUD
{
    public class LocoIndicatorReader : MonoBehaviour
    {
        public IndicatorValue tmTemp;
        public IndicatorValue oilTemp;
        public IndicatorValue amps;
        public IndicatorValue engineRpm;
    }

    public class IndicatorValue
    {
        public float Value => 0f;

        // The real indicator type is truthy (Unity Object); used as
        // `if (IndicatorReader?.tmTemp)` in DVTrainCarWrapper.
        public static implicit operator bool(IndicatorValue value) => false;
    }
}

namespace DV.Wheels
{
    public class AdhesionController : MonoBehaviour
    {
        public WheelslipControllerOption wheelslipController = new WheelslipControllerOption();
    }

    public class WheelslipControllerOption
    {
        public bool IsSome(out WheelslipController controller)
        {
            controller = new WheelslipController();
            return false;
        }
    }

    public class WheelslipController
    {
        public bool IsWheelslipping => false;
        public float wheelslip => 0f;
    }
}

namespace DV.Utils
{
    public class SingletonBehaviour<T> where T : class
    {
        public static T Instance => null;
    }
}

namespace DV.Simulation.Cars
{
    // TrainCar lives in the global namespace (Assembly-CSharp); this only
    // satisfies the using in DVTrainCarWrapper against the real game DLL.
    internal sealed class CarsNamespaceAnchor { }
}

namespace I2.Loc
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage => "English";
    }
}
