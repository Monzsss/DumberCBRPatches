using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Niel2), "InitializeTrait")]
    public class NielInitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        public static bool Prefix(Niel2 __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Niel";

            var cfg = ModEntry.CharacterConfig?.Values["Niel2"];
            //var cfg = new CharacterConfigEntries("Niel2");

            if (cfg == null)
                return true;

            __instance.TitsType = Clamp(cfg.TitsType, 0, 0);

            var dict = cfg.ParseTraitsAsDictionary();

            foreach (var kv in dict)
            {
                __instance.AddTrait(kv.Key);
                __instance.AddTraitValue(kv.Key, kv.Value);
            }

            return false;
        }
    }
}
