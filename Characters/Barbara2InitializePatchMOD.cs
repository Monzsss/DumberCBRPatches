using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Barbara2), "InitializeTrait")]
    public class Barbara2InitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        public static bool Prefix(Barbara2 __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Barbara";

            //var cfg = new CharacterConfigEntries("Barbara2");
            var cfg = ModEntry.CharacterConfig?.Values["Barbara2"];

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
