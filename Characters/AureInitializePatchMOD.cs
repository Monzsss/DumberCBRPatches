using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Aure), "InitializeTrait")]
    public class AureInitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        
        public static bool Prefix(Aure __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Aure";

            //var cfg = new CharacterConfigEntries("Aure");
            var cfg = ModEntry.CharacterConfig?.Values["Aure"];

            if (cfg == null)
                return true;

            __instance.TitsType = Clamp(cfg.TitsType, 0, 4);

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
