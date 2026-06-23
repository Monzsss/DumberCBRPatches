using System;
using System.Collections.Generic;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBM.ModLoader.Settings;
using MBMScripts;

namespace DumberCBRPatches.Player
{
    [HarmonyPatch(typeof(MBMScripts.Player), "InitializeTrait", new Type[] { })]
    public class InitializeTraitPatch
    {
        public static void Postfix(MBMScripts.Player __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();

            var pCfg = global::DumberCBRPatches.ModEntry.PlayerConfig;
            if (pCfg == null)
                return;

            // Assign ConceptionRate directly from configuration
            __instance.ConceptionRate = pCfg.ConceptionRate;

            // Loop through traits to apply them visually/mechanically to the character data
            foreach (var (trait, points) in pCfg.ParseTraits())
            {
                __instance.AddTrait(trait);
                __instance.AddTraitValue(trait, points);

                // Note: The mathematical calculations affecting baseSexTime are removed 
                // because your config value is now treated as an absolute priority.
            }

            // FIX 1: Lock the final SexTime strictly to your config value (e.g., 5s)
            // Traits will no longer reduce or increase this duration.
            __instance.SexTime = Math.Max(0f, pCfg.SexTime);
        }
    }
}
