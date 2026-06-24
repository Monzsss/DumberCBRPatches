using System;
using System.Linq;
using System.Collections.Generic;
using MBMScripts;
using MBM.ModLoader.Settings;

namespace DumberCBRPatches.Configuration
{
    public class SpeciesConfigValues
    {
        private static string ModName => ModEntry.ModName;

        public int GetBaseChance(string spName)
        {
            return ModSettings.GetInt(ModName, $"Species_{spName}_BaseChance");
        }

        public string[] GetPossiblyMothers(string spName)
        {
            string csv = ModSettings.GetString(ModName, $"Species_{spName}_PossiblyMothers");
            return ParseCsv(csv).ToArray();
        }

        public ERace[] GetPossiblyFathers(string spName)
        {
            string csv = ModSettings.GetString(ModName, $"Species_{spName}_PossiblyFathers");

            return ParseCsv(csv)
                .Where(s => Enum.IsDefined(typeof(ERace), s))
                .Select(s => (ERace)Enum.Parse(typeof(ERace), s))
                .ToArray();
        }

        private IEnumerable<string> ParseCsv(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
                return Enumerable.Empty<string>();

            return csvContent
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));
        }
    }
}