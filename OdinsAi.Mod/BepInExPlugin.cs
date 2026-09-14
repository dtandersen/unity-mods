using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using OdinsAi.Core;
using UnityEngine;

namespace OdinsAi.Mod
{
    /// <summary>
    /// Odin's Eye BepInEx plugin. The Unity/BepInEx adapter layer; actual logic
    /// lives in OdinsAi.Core (netstandard2.0) and is specified under OdinsAi.Test.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class BepInExPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "odinsai";
        public const string ModName = "Odin's Eye";
        public const string ModVersion = "0.1.0";

        public static BepInExPlugin context;

        private Harmony harmony;

        public static ConfigEntry<bool> modEnabled;

        public static void Dbgl(string str = "", bool pref = true)
        {
            Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }

        public void Awake()
        {
            context = this;

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Odin's Eye");

            if (!modEnabled.Value)
                return;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            Dbgl($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Terminal), "InputText")]
        private static class Terminal_InputText_Patch
        {
            private static bool Prefix(Terminal __instance)
            {
                if (!modEnabled.Value)
                    return true;

                switch (__instance.m_input.text.Trim().ToLower())
                {
                    case "odinsai reset":
                        context.Config.Reload();
                        context.Config.Save();
                        __instance.AddString($"{ModName}: config reloaded");
                        return false;

                    case "odinsai status":
                        __instance.AddString($"{ModName} v{ModVersion} (core {CoreAssembly.Version})");
                        return false;
                }

                return true;
            }
        }
    }
}