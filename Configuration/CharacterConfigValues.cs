//CharacterConfigValues.cs

using System.Globalization;
using MBM.ModLoader.Settings;
using MBMScripts;

namespace DumberCBRPatches.Configuration
{
    public class CharacterConfigValues
    {
        public Dictionary<string, CharacterConfigEntries> Values { get; }
            = new(StringComparer.OrdinalIgnoreCase);

        public CharacterConfigValues()
        {
            foreach (var name in ModSettingsDataRegister.TargetCharacters)
            {
                var entry = new CharacterConfigEntries(name);
                Values[name] = entry;

                string cleanName = ToDisplayNameHelper(name);
                if (!Values.ContainsKey(cleanName))
                {
                    Values[cleanName] = entry;
                }
            }
        }

        private static string ToDisplayNameHelper(string id)
        {
            for (int i = id.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(id[i]))
                    return id.Substring(0, i + 1);
            }
            return id;
        }
    }

    public class CharacterConfigEntries
    {
        private const string BaseModName = "DumberCBRPatches";

        public string CharacterName { get; }

        public CharacterConfigEntries(string characterName)
        {
            CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        }

        // =========================================================
        // TITS TYPE
        // =========================================================
        public int TitsType
        {
            get
            {
                if (ModSettingsDataRegister.CharacterTitsDropdowns.TryGetValue(CharacterName, out var dropdown))
                {
                    var val = dropdown.Value;
                    return val < 0 ? 0 : (val > 5 ? 5 : val);
                }

                string cleanName = ToCleanName(CharacterName);
                if (ModSettingsDataRegister.CharacterTitsDropdowns.TryGetValue(cleanName, out dropdown))
                {
                    var val = dropdown.Value;
                    return val < 0 ? 0 : (val > 5 ? 5 : val);
                }

                return 4;
            }
        }

        // =========================================================
        // TRAITS STRING
        // =========================================================
        public string Traits
        {
            get
            {
                string cleanName = ToCleanName(CharacterName);
                string settingsKey = $"{cleanName} Essence";

                return ModSettings.GetString(BaseModName, settingsKey) ?? string.Empty;
            }
        }

        // =========================================================
        // TRAIT MAP
        // =========================================================
        private static readonly Dictionary<string, ETrait> TraitMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Demonic", ETrait.Trait93 },
                { "Sacred", ETrait.Trait94 },
                { "Elemental", ETrait.Trait95 },
                { "Eternal", ETrait.Trait96 },
                { "Magical", ETrait.Trait97 },
                { "Feral", ETrait.Trait98 }
            };

        private static readonly Dictionary<ETrait, float> DefaultTraits =
            new()
            {
                { ETrait.Trait93, 0f },
                { ETrait.Trait94, 0f },
                { ETrait.Trait95, 0f },
                { ETrait.Trait96, 0f },
                { ETrait.Trait97, 0f },
                { ETrait.Trait98, 0f }
            };

        // =========================================================
        // CACHE
        // =========================================================
        private Dictionary<ETrait, float>? _traitCache;

        public void InvalidateCache()
        {
            _traitCache = null;
        }

        private Dictionary<ETrait, float> GetTraitsCached()
        {
            if (_traitCache != null)
                return _traitCache;

            _traitCache = BuildTraits();
            return _traitCache;
        }

        // =========================================================
        // BUILD TRAITS
        // =========================================================
        private Dictionary<ETrait, float> BuildTraits()
        {
            var result = new Dictionary<ETrait, float>(DefaultTraits);

            string raw = Traits;
            if (string.IsNullOrWhiteSpace(raw))
                return result;

            string[] pairs = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                string trimmed = pair.Trim();
                if (trimmed.Length == 0)
                    continue;

                string[] parts = trimmed.Split(new[] { '=', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!TraitMap.TryGetValue(key, out var trait))
                    continue;

                if (float.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                {
                    result[trait] = value;
                }
            }

            return result;
        }

        // =========================================================
        // PUBLIC API
        // =========================================================
        public float GetTraitValue(ETrait trait)
        {
            var dict = GetTraitsCached();
            return dict.TryGetValue(trait, out float v) ? v : 0f;
        }

        public bool HasTrait(ETrait trait, float min = 0.1f)
        {
            return GetTraitValue(trait) >= min;
        }

        public Dictionary<ETrait, float> ParseTraitsAsDictionary()
        {
            return new Dictionary<ETrait, float>(GetTraitsCached());
        }

        private static string ToCleanName(string id)
        {
            for (int i = id.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(id[i]))
                    return id.Substring(0, i + 1);
            }
            return id;
        }
    }
}

