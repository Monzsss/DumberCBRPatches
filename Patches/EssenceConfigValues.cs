using System;
using System.Collections.Generic;
using System.Reflection;
using MBMScripts;

namespace DumberCBRPatches.Configuration
{
    public struct LocalUpgradeAttributeInfo
    {
        public int Attribute { get; set; }
        public float UpgradeValue { get; set; }
        public float UpgradePointsPerValue { get; set; }
    }

    public class EssenceConfigValues
    {
        public Dictionary<ETrait, List<LocalUpgradeAttributeInfo>> EssenceDataAttributes { get; internal set; } = new();

        /// <summary>
        /// Reads the internal Redux EssenceRegistry using reflection, contextually processes 
        /// negative mechanics into buffs if selected, and populates our local clean storage.
        /// </summary>
        public void PopulationRegistryCacheFromRedux()
        {
            EssenceDataAttributes.Clear();

            try
            {
                Type? registryType = Type.GetType("ComplexBreedingRedux.Data.Essences.EssenceRegistry, ComplexBreedingRedux");
                if (registryType == null) return;

                PropertyInfo? allProp = registryType.GetProperty("All", BindingFlags.Public | BindingFlags.Static);
                var allCollection = allProp?.GetValue(null) as System.Collections.IEnumerable;
                if (allCollection == null) return;

                int filterMode = ModSettingsDataRegister.EssenceFilterModeData.Value;

                foreach (object essence in allCollection)
                {
                    if (essence == null) continue;

                    Type essenceType = essence.GetType();
                    PropertyInfo? traitProp = essenceType.GetProperty("ReplacedTrait") ?? essenceType.GetProperty("Trait");
                    PropertyInfo? upgradesProp = essenceType.GetProperty("AttributeUpgrades");

                    if (traitProp == null || upgradesProp == null) continue;

                    var traitValue = traitProp.GetValue(essence);
                    if (traitValue is ETrait eTrait)
                    {
                        var sourceUpgrades = upgradesProp.GetValue(essence) as System.Collections.IEnumerable;
                        if (sourceUpgrades == null) continue;

                        var processedList = new List<LocalUpgradeAttributeInfo>();

                        foreach (object upgrade in sourceUpgrades)
                        {
                            if (upgrade == null) continue;

                            Type upType = upgrade.GetType();

                            // Safely extract names and base numeric metrics
                            var attrObj = upType.GetProperty("Attribute")?.GetValue(upgrade);
                            string attrName = attrObj?.ToString() ?? string.Empty;
                            int attributeEnumInt = (int)(attrObj ?? 0);

                            float upValue = (float)(upType.GetProperty("UpgradeValue")?.GetValue(upgrade) ?? 0f);
                            float pointsPerVal = (float)(upType.GetProperty("UpgradePointsPerValue")?.GetValue(upgrade) ?? 0f);

                            // 1. CONTEXTUAL ASSESSMENT: Check whether the entry hurts the character
                            bool isNegativeEffect = false;

                            if (attrName == "MaintenanceCost")
                            {
                                isNegativeEffect = upValue > 0f; // Paying more gold is a penalty
                            }
                            else if (attrName == "GrowthTime" || attrName == "DamageDuringSex" || attrName == "SexTime")
                            {
                                isNegativeEffect = upValue > 0f; // Higher values slow down or hurt characters
                            }
                            else
                            {
                                isNegativeEffect = upValue < 0f; // Standard negative numbers are debuffs
                            }

                            // 2. APPLY UI TWOTIER CONTROLS (0 = Allow, 1 = Invert)
                            if (isNegativeEffect && filterMode == 1)
                            {
                                // Flip the mathematical polarity around to turn the penalty into a beneficial buff
                                upValue = -upValue;
                            }

                            processedList.Add(new LocalUpgradeAttributeInfo
                            {
                                Attribute = attributeEnumInt,
                                UpgradeValue = upValue,
                                UpgradePointsPerValue = pointsPerVal
                            });
                        }

                        EssenceDataAttributes[eTrait] = processedList;
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.LogError($"Failed to load and contextually filter Redux database cache: {ex}");
            }
        }

        internal ICollection<LocalUpgradeAttributeInfo> GetEssenceAttributeInfo(ETrait essenceTrait)
        {
            if (EssenceDataAttributes != null && EssenceDataAttributes.TryGetValue(essenceTrait, out var sourceList))
            {
                return sourceList;
            }
            return Array.Empty<LocalUpgradeAttributeInfo>();
        }
    }
}
