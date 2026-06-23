using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DumberCBRPatches.Patches
{
    [HarmonyPatch]
    public static class ProfessionPatch
    {
        private static bool _patched;

        // =========================
        // CALLED FROM MODENTRY ONLY
        // =========================
        public static void ApplyAll()
        {
            if (_patched) return;

            try
            {
                Type gameDataType = AccessTools.TypeByName("MBMScripts.GameData");

                if (gameDataType == null)
                {
                    Debug.LogWarning("[DumberCBRPatches] GameData not found.");
                    return;
                }

                FieldInfo dataListField =
                    AccessTools.Field(gameDataType, "ProfessionDataList");

                if (dataListField == null)
                {
                    Debug.LogWarning("[DumberCBRPatches] ProfessionDataList not found.");
                    return;
                }

                IList? professionList = dataListField.GetValue(null) as IList;

                if (professionList == null || professionList.Count == 0)
                {
                    Debug.LogWarning("[DumberCBRPatches] Profession list empty.");
                    return;
                }

                var config = ModEntry.ProfessionConfig;

                if (config == null)
                {
                    Debug.LogWarning("[DumberCBRPatches] Profession config missing.");
                    return;
                }

                int patchedCount = 0;

                foreach (object prof in professionList)
                {
                    if (prof == null) continue;

                    Type profType = prof.GetType();
                    string typeName = profType.Name;

                    bool isTarget =
                        typeName == "PaladinProfessionData" ||
                        typeName == "MaidProfessionData" ||
                        typeName == "WarriorProfessionData" ||
                        typeName == "WizardProfessionData";

                    if (!isTarget) continue;

                    string profName =
                        AccessTools.Property(profType, "Name")?
                        .GetValue(prof) as string ?? string.Empty;

                    Debug.Log($"[DumberCBRPatches] Patching profession: {profName}");

                    switch (typeName)
                    {
                        case "PaladinProfessionData":
                            PatchPaladin(prof, profType, config);
                            break;

                        case "MaidProfessionData":
                            PatchMaid(prof, profType, config);
                            break;

                        case "WarriorProfessionData":
                            PatchWarrior(prof, profType, config);
                            break;

                        case "WizardProfessionData":
                            PatchWizard(prof, profType, config);
                            break;
                    }

                    PatchJobs(prof, profType, profName, config);

                    patchedCount++;
                }

                _patched = true;

                Debug.Log($"[DumberCBRPatches] Successfully patched {patchedCount} professions.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] ApplyAll error: {ex}");
            }
        }

        // =========================
        // PROFESSION PATCHES
        // =========================

        private static void PatchPaladin(object prof, Type profType, dynamic config)
        {
            try
            {
                var forbiddenSpecies =
                    AccessTools.Field(profType, "ForbiddenStartingSpecies")
                    ?.GetValue(prof) as IList;

                forbiddenSpecies?.Clear();

                AccessTools.Field(profType, "Teachable")
                    ?.SetValue(prof, config.GetTeachable("Paladin"));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] Paladin error: {ex.Message}");
            }
        }

        private static void PatchMaid(object prof, Type profType, dynamic config)
        {
            try
            {
                AccessTools.Field(profType, "Teachable")
                    ?.SetValue(prof, config.GetTeachable("Maid"));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] Maid error: {ex.Message}");
            }
        }

        private static void PatchWarrior(object prof, Type profType, dynamic config)
        {
            try
            {
                AccessTools.Field(profType, "Teachable")
                    ?.SetValue(prof, config.GetTeachable("Warrior"));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] Warrior error: {ex.Message}");
            }
        }

        private static void PatchWizard(object prof, Type profType, dynamic config)
        {
            try
            {
                AccessTools.Field(profType, "Teachable")
                    ?.SetValue(prof, config.GetTeachable("Wizard"));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] Wizard error: {ex.Message}");
            }
        }

        // =========================
        // JOB PATCHES
        // =========================

        private static void PatchJobs(object prof, Type profType, string profName, dynamic config)
        {
            try
            {
                var jobs =
                    AccessTools.Field(profType, "Jobs")
                    ?.GetValue(prof) as IList;

                if (jobs == null || jobs.Count == 0)
                    return;

                foreach (object job in jobs)
                {
                    if (job == null) continue;

                    Type jobType = job.GetType();

                    string jobName =
                        AccessTools.Property(jobType, "Name")
                        ?.GetValue(job) as string ?? string.Empty;

                    var valueField = AccessTools.Field(jobType, "Value");
                    var maxField = AccessTools.Field(jobType, "ValueMax");
                    var chanceField = AccessTools.Field(jobType, "Chance");

                    switch (jobName)
                    {
                        case "Adventuring":
                            valueField?.SetValue(job, 3600f);
                            maxField?.SetValue(job, 12000f);
                            chanceField?.SetValue(job, config.GetBaseChance(profName));
                            break;

                        case "Soul Cleansing":
                            if (profType.Name == "PaladinProfessionData")
                            {
                                valueField?.SetValue(job, 40f);
                                maxField?.SetValue(job, 60f);
                            }
                            break;

                        case "Casting Healing Spells":
                            if (profType.Name == "PaladinProfessionData")
                            {
                                valueField?.SetValue(job, 75f);
                                maxField?.SetValue(job, 120f);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DumberCBRPatches] Job error: {ex.Message}");
            }
        }
    }
}