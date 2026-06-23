using DumberCBRPatches;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRRPatches.Characters
{
    [HarmonyPatch(typeof(Bella), "InitializeTrait")]
    public class BellaInitializePatchMOD
    {
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }
        public static bool Prefix(Bella __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Bella";

            var cfg = ModEntry.CharacterConfig?.Values["Bella"];
            //var cfg = new CharacterConfigEntries("Bella");

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
