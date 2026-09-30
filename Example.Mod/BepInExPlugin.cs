using BepInEx;

namespace Example.Mod
{
    /// <summary>
    /// Minimal smoke-test plugin. Proves the build resolves BepInEx, Harmony,
    /// Stationeers (Assembly-CSharp) and Unity assemblies, and deploys to the
    /// BepInEx plugins folder. Replace with your real mod.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class BepInExPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "example.stationeersmod";
        public const string ModName = "Example Stationeers Mod";
        public const string ModVersion = "0.1.0";

        public void Awake()
        {
            Logger.LogInfo($"{ModName} {ModVersion} loaded.");
        }
    }
}
