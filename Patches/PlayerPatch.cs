//PlayerPatch.cs

using HarmonyLib;

namespace DumberCBRPatches.Player
{
    [HarmonyPatch(typeof(MBMScripts.Player), "InitializeTrait", [])]
    public class InitializeTraitPatch
    {
        public static void Postfix(MBMScripts.Player __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();

            var pCfg = ModEntry.PlayerConfig;
            if (pCfg == null)
                return;

            __instance.ConceptionRate = pCfg.ConceptionRate;

            foreach (var (trait, points) in pCfg.ParseTraits())
            {
                __instance.AddTrait(trait);
                __instance.AddTraitValue(trait, points);

            }

            __instance.SexTime = Math.Max(0f, pCfg.SexTime);
        }
    }
}
