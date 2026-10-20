// Minimal Derail Valley fakes: only what DriverAssist compiles against.
// Signatures mirror the real game API; bodies are never run (CI compile only).
using System;
using System.Collections.Generic;
using DV.Simulation.Brake;
using DV.Wheels;
using UnityEngine;

public class TrainCar : MonoBehaviour
{
    public Trainset trainset;
    public BrakeSystem brakeSystem;
    public GameObject interior;
    public GameObject loadedInterior;
    public string carType;
    public CarLivery carLivery;
    public MassController massController;
    public bool IsLoco;
    public AdhesionController adhesionController;

    public float GetForwardSpeed() => 0f;
}

public class Trainset
{
    public List<TrainCar> cars = new List<TrainCar>();
}

public class MassController
{
    public float TotalMass => 0f;
}

public class CarLivery
{
    public ParentType parentType;
}

public class ParentType
{
    public float wheelRadius => 0f;
}

public class InteriorControlsManager : MonoBehaviour
{
    public enum ControlType
    {
        GearboxA,
        GearboxB,
    }

    public bool TryGetControl(ControlType type, out ControlReference reference)
    {
        reference = new ControlReference();
        return false;
    }
}

public class ControlReference
{
    public ControlImplBase controlImplBase = new ControlImplBase();
}

public class ControlImplBase
{
    public float Value => 0f;
    public void SetValue(float value) { }
}

public static class PlayerManager
{
    public static Transform PlayerTransform => null;
    public static TrainCar Car => null;
    public static event Action<TrainCar> CarChanged;
}

public class PlayerCameraSwitcher : MonoBehaviour
{
    public ExternalCamera externalCamera;
}

public class ExternalCamera : MonoBehaviour
{
    public event Action<bool> PhotoModeChanged;
}

public static class WorldStreamingInit
{
    public static event Action LoadingFinished;
}

public static class UnloadWatcher
{
    public static event Action UnloadRequested;
}

public static class DVGUI
{
    public static GUISkin skin => null;
}
