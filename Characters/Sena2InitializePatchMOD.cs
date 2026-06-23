using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Sena2), "InitializeTrait")]
    public class Sena2InitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        public static bool Prefix(Sena2 __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Sena";

            var cfg = ModEntry.CharacterConfig?.Values["Sena2"];
            //var cfg = new CharacterConfigEntries("Sena2");

            if (cfg == null)
                return true;

            __instance.TitsType = Clamp(cfg.TitsType, 0, 3);

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
