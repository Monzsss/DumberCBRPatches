//AttributeUpgradesPatch.cs

using HarmonyLib;
using System.Collections;
using System.Reflection;
using MBMScripts;
using DumberCBRPatches.Configuration;

namespace DumberCBRPatches.Patches
{
    public static class AttributeUpgradesPatch
    {
        private static bool _patched = false;
        private static readonly Dictionary<ETrait, IEnumerable> PrebuiltListsCache = [];

        public static void ApplyManualPatch(Harmony harmonyInstance)
        {
            if (_patched) return;

            try
            {
                Type? essenceDataType = Type.GetType("ComplexBreedingRedux.Data.Essences.EssenceData, ComplexBreedingRedux");
                if (essenceDataType == null)
                {
                    ModEntry.LogError("Could not find ComplexBreedingRedux EssenceData type for attribute patching.");
                    return;
                }

                PropertyInfo? upgradesProp = essenceDataType.GetProperty("AttributeUpgrades", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo? targetGetter = upgradesProp?.GetGetMethod();

                if (targetGetter == null)
                {
                    ModEntry.LogError("Could not locate AttributeUpgrades property getter in Redux mod.");
                    return;
                }

                MethodInfo postfixMethod = typeof(AttributeUpgradesPatch).GetMethod(nameof(FilterReduxAttributesPostfix), BindingFlags.Static | BindingFlags.NonPublic);
                harmonyInstance.Patch(targetGetter, postfix: new HarmonyMethod(postfixMethod));

                _patched = true;
                ModEntry.Log("Dynamically hooked two-tier Redux AttributeUpgrades stream successfully.");
            }
            catch (Exception ex)
            {
                ModEntry.LogError($"Failed to inject custom property modifier: {ex}");
            }
        }

        public static void UpdateCachedPayload(ETrait trait, IEnumerable filteredList)
        {
            PrebuiltListsCache[trait] = filteredList;
        }

        public static void ClearCache()
        {
            PrebuiltListsCache.Clear();
        }

        private static void FilterReduxAttributesPostfix(object __instance, ref IEnumerable __result)
        {
            if (__instance == null || ModSettingsDataRegister.EssenceFilterModeData.Value == 0) return;

            if (EssenceConfigValues.TryGetTraitFromInstance(__instance, out ETrait trait))
            {
                if (PrebuiltListsCache.TryGetValue(trait, out var cachedList))
                {
                    __result = cachedList;
                }
            }
        }
    }
}
