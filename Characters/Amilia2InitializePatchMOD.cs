using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Amilia2), "InitializeTrait")]
    public class Amilia2InitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        
        public static bool Prefix(Amilia2 __instance)
        {

            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Amilia";

            //var cfg = new CharacterConfigEntries("Amilia2");
            var cfg = ModEntry.CharacterConfig?.Values["Amilia2"];

            if (cfg == null)
                return true;

            __instance.TitsType = Clamp(cfg.TitsType, 0, 5);

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