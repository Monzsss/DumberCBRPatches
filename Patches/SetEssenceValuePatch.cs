using HarmonyLib;
using MBMScripts;
using System;
using System.Reflection;
using DumberCBRPatches.Configuration;

namespace DumberCBRPatches.Patches
{
    public class SetEssenceValuePatch
    {
        private static bool _patched = false;

        public static void ApplyManualPatch(Harmony harmonyInstance)
        {
            if (_patched) return;

            try
            {
                MethodInfo? targetMethod = null;

                // Scan every assembly loaded in the game domain to hunt down SetEssenceValue
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        // Check if the class contains our target method signature
                        MethodInfo? method = type.GetMethod("SetEssenceValue",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                            null,
                            new Type[] { typeof(Character), typeof(Character), typeof(Character), typeof(ETrait) },
                            null);

                        if (method != null)
                        {
                            targetMethod = method;
                            break;
                        }
                    }
                    if (targetMethod != null) break;
                }

                if (targetMethod == null)
                {
                    global::DumberCBRPatches.ModEntry.LogError("CRITICAL: Failed to locate any SetEssenceValue signature in the entire game engine!");
                    return;
                }

                // Inject our custom prefix blocker
                MethodInfo prefixMethod = typeof(SetEssenceValuePatch).GetMethod(nameof(PrefixBypass), BindingFlags.Static | BindingFlags.Public);
                harmonyInstance.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));

                _patched = true;
                global::DumberCBRPatches.ModEntry.Log($"[SUCCESS] Intercepted and shut down the Redux breeding system at: {targetMethod.DeclaringType?.FullName}.SetEssenceValue");
            }
            catch (Exception ex)
            {
                global::DumberCBRPatches.ModEntry.LogError($"Breeding system injection failure: {ex}");
            }
        }

        /// <summary>
        /// This method intercepts the birth. Returning FALSE completely erases the Redux math, 
        /// preventing values from ballooning into 30, 40, etc.
        /// </summary>
        public static bool PrefixBypass(Character child, Character mainParent, Character subParent, ETrait essence)
        {
            try
            {
                int filterMode = ModSettingsDataRegister.EssenceFilterModeData.Value;

                // 1. EXTRACT RAW PARENT RATINGS
                int mainParentVal = (int)mainParent.GetTraitValue(essence) + (int)mainParent.GetTraitUpgradedValue(essence);
                int subParentVal = (int)subParent.GetTraitValue(essence) + (int)subParent.GetTraitUpgradedValue(essence);

                if (mainParentVal == 0 && subParentVal == 0) return false;

                // 2. STOP RUNAWAY ADDITIONS: Normalize stats down by taking the parental average
                // Instead of 15 + 15 = 30, it turns it into: (15 + 15) / 2 = 15
                int inheritedValue = (int)Math.Round((mainParentVal + subParentVal) / 2.0);

                // Minor 30% genetic variety upgrade roll
                System.Random rng = new System.Random();
                if (rng.Next(1, 101) <= 30)
                {
                    inheritedValue += 1;
                }

                // 3. IDENTIFY CONTEXTUAL MECHANIC DEBUFFS
                string essenceName = essence.ToString();
                bool isNegativeEffect = false;

                if (essenceName == "MaintenanceCost" || essenceName == "GrowthTime" || essenceName == "DamageDuringSex" || essenceName == "SexTime")
                {
                    isNegativeEffect = inheritedValue > 0;
                }
                else
                {
                    isNegativeEffect = inheritedValue < 0;
                }

                // 4. PROCESS YOUR TWO-TIER FILTER SELECTION
                if (isNegativeEffect && filterMode == 1) // "Invert Negatives to Buffs"
                {
                    inheritedValue = -inheritedValue;
                }

                // 5. THE ULTIMATE EMERGENCY CLAMP
                // This acts as a brick wall. It compresses the data payload, forcing numbers to stay between -10 and 10!
                int finalSafeValue = Math.Max(-10, Math.Min(inheritedValue, 10));

                if (finalSafeValue != 0)
                {
                    child.AddTrait(essence);
                    child.AddTraitValue(essence, (float)finalSafeValue);
                }
            }
            catch (Exception ex)
            {
                global::DumberCBRPatches.ModEntry.LogError($"Failed to cleanly execute custom child breeding logic: {ex}");
            }

            return false; // Crucial: Stops the original Redux code from running, killing the 40 bug!
        }
    }
}
