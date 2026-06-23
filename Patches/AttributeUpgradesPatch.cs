using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using MBMScripts;
using DumberCBRPatches.Configuration;

namespace DumberCBRPatches.Patches
{
    public class AttributeUpgradesPatch
    {
        private static bool _patched = false;

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

        private static void FilterReduxAttributesPostfix(object __instance, ref System.Collections.IEnumerable __result)
        {
            if (__result == null) return;

            int filterMode = ModSettingsDataRegister.EssenceFilterModeData.Value;

            // FIX: If user wants vanilla behavior (0: Allow Negatives), exit early without executing reflection copies
            if (filterMode == 0) return;

            Type listType = __result.GetType();
            if (!listType.IsGenericType) return;

            Type itemType = listType.GetGenericArguments()[0];

            Type genericListType = typeof(List<>).MakeGenericType(itemType);
            var filteredList = Activator.CreateInstance(genericListType) as System.Collections.IList;
            if (filteredList == null) return;

            PropertyInfo? attrProp = itemType.GetProperty("Attribute");
            PropertyInfo? valueProp = itemType.GetProperty("UpgradeValue");
            PropertyInfo? pointsProp = itemType.GetProperty("UpgradePointsPerValue");

            foreach (object originalUpgradeItem in __result)
            {
                if (originalUpgradeItem == null) continue;

                string attrName = attrProp?.GetValue(originalUpgradeItem)?.ToString() ?? string.Empty;
                float rawValue = (float)(valueProp?.GetValue(originalUpgradeItem) ?? 0f);

                bool isNegativeEffect = false;

                if (attrName == "MaintenanceCost")
                {
                    isNegativeEffect = rawValue > 0f;
                }
                else if (attrName == "GrowthTime" || attrName == "DamageDuringSex" || attrName == "SexTime")
                {
                    isNegativeEffect = rawValue > 0f;
                }
                else
                {
                    isNegativeEffect = rawValue < 0f;
                }

                object clonedUpgradeItem = Activator.CreateInstance(itemType);
                attrProp?.SetValue(clonedUpgradeItem, attrProp.GetValue(originalUpgradeItem));
                pointsProp?.SetValue(clonedUpgradeItem, pointsProp.GetValue(originalUpgradeItem));

                // FIX: Since filterMode 0 exits early, arriving here guarantees filterMode is 1 (Invert Negatives)
                if (isNegativeEffect)
                {
                    // Flips the mathematical logic around to make the penalty act as a buff
                    valueProp?.SetValue(clonedUpgradeItem, -rawValue);
                }
                else
                {
                    valueProp?.SetValue(clonedUpgradeItem, rawValue);
                }

                filteredList.Add(clonedUpgradeItem);
            }

            __result = filteredList;
        }
    }
}
