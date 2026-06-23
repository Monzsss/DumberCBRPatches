using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using MBM.ModLoader.Settings;
using DumberCBRPatches.Configuration;

namespace DumberCBRPatches
{
    public static class ModEntry
    {
        public const string ModId = "DumberCBRPatches";
        public const string ModTitle = "Dumber CBR Patches";
        public const string ModVersion = "1.0.0";
        public const string ModName = ModId;

        public static CharacterConfigValues? CharacterConfig { get; private set; }
        public static ProfessionConfigValues? ProfessionConfig { get; private set; }
        public static SpeciesConfigValues? SpeciesConfig { get; private set; }
        public static PlayerConfigValues? PlayerConfig { get; private set; }

        // Explicit global public definition
        public static EssenceConfigValues? EssenceConfig { get; private set; }

        private static bool _initialized;
        private static bool _resetInProgress;

        public static void Log(string msg) =>
            Debug.Log($"[{ModTitle}] {msg}");

        public static void LogError(string msg) =>
            Debug.LogError($"[{ModTitle}] {msg}");

        public static void Load()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                Log($"Loading {ModTitle} v{ModVersion}...");

                ModSettingsDataRegister.Initialize();

                ReloadAllConfig();

                RegisterCustomModUi();
                RegisterSettingsListeners();

                var harmony = new Harmony(ModId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                DumberCBRPatches.Patches.AttributeUpgradesPatch.ApplyManualPatch(harmony);
                //DumberCBRPatches.Patches.SetEssenceValuePatch.ApplyManualPatch(harmony);

                Log("Mod loaded successfully.");
            }
            catch (Exception ex)
            {
                LogError($"Fatal execution failure: {ex}");
            }
        }

        private static void ReloadAllConfig()
        {
            CharacterConfig = new CharacterConfigValues();
            ProfessionConfig = new ProfessionConfigValues();
            SpeciesConfig = new SpeciesConfigValues();
            PlayerConfig = new PlayerConfigValues();

            // FIX: Explicitly assign through the master ModEntry class scope pointer to resolve CS0103!
            var essenceCfg = new EssenceConfigValues();
            essenceCfg.PopulationRegistryCacheFromRedux();
            ModEntry.EssenceConfig = essenceCfg;

            Log("Config cache rebuilt and Redux negative modifiers filtered.");
        }

        private static void RegisterCustomModUi()
        {
            try
            {
                ModSettings.RegisterDropdown(
                    ModName,
                    "Reset_Config",
                    new[] { "Custom", "Reset" },
                    0,
                    "Reset all DumberCBR settings",
                    "Utilities"
                );

                Log("Custom UI registered.");
            }
            catch (Exception ex)
            {
                LogError($"UI register failed: {ex}");
            }
        }

        private static void RegisterSettingsListeners()
        {
            try
            {
                ModSettings.OnChanged(ModName, "Reset_Config", v =>
                {
                    if (_resetInProgress)
                        return;

                    if ((int)v != 1)
                        return;

                    _resetInProgress = true;

                    try
                    {
                        Log("Reset triggered");

                        ModSettingsDataRegister.ExecuteGlobalReset();

                        ReloadAllConfig();

                        if (CharacterConfig != null)
                        {
                            foreach (var cfg in CharacterConfig.Values.Values)
                            {
                                cfg.InvalidateCache();
                            }
                        }

                        ModSettings.Set(ModName, "Reset_Config", 0);

                        Log("Reset complete");
                    }
                    finally
                    {
                        _resetInProgress = false;
                    }
                });

                // Automated trackers for custom characters
                foreach (var name in ModSettingsDataRegister.TargetCharacters)
                {
                    ModSettings.OnChanged(ModName, $"{name}_Traits", _ =>
                    {
                        if (_resetInProgress) return;
                        ReloadAllConfig();
                    });

                    ModSettings.OnChanged(ModName, $"{name}_TitsType", _ =>
                    {
                        if (_resetInProgress) return;
                        ReloadAllConfig();
                    });
                }

                string[] watchedKeys =
                {
                    "Player_SexTime",
                    "Player_ConceptionRate",
                    "Player_Traits",
                    "Essence_FilterMode"
                };

                foreach (var key in watchedKeys)
                {
                    ModSettings.OnChanged(ModName, key, _ =>
                    {
                        if (_resetInProgress)
                            return;

                        ReloadAllConfig();
                    });
                }

                Log("Settings listeners initialized.");
            }
            catch (Exception ex)
            {
                LogError($"Listener init failed: {ex}");
            }
        }
    }
}