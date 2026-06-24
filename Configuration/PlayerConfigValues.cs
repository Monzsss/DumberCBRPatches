//PlayerConfigValues.cs

using System.Globalization;
using MBMScripts;

namespace DumberCBRPatches.Configuration
{
    public class PlayerConfigValues
    {
        public float SexTime => ModSettingsDataRegister.SexTimeData.Value;

        public float ConceptionRate => ModSettingsDataRegister.ConceptionRateData.Value switch
        {
            0 => 0f,
            1 => 0.25f,
            2 => 0.5f,
            3 => 0.75f,
            _ => 1f
        };

        public string Traits => ModSettingsDataRegister.PlayerTraitsData.Value ?? string.Empty;

        public IEnumerable<(ETrait trait, float value)> ParseTraits()
        {
            var raw = Traits;

            if (string.IsNullOrWhiteSpace(raw))
                yield break;

            var traitMap = new Dictionary<string, ETrait>(StringComparer.OrdinalIgnoreCase)
            {
                { "Demonic", ETrait.Trait93 },
                { "Sacred", ETrait.Trait94 },
                { "Elemental", ETrait.Trait95 },
                { "Eternal", ETrait.Trait96 },
                { "Magical", ETrait.Trait97 },
                { "Feral", ETrait.Trait98 }
            };

            foreach (var p in raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = p.Split(['=', ':'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                string traitKey = parts[0].Trim();
                string valStr = parts[1].Trim();

                if (!traitMap.TryGetValue(traitKey, out var trait))
                    continue;

                if (!float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    continue;

                yield return (trait, val);
            }
        }
    }
}
