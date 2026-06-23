using System;
using System.Collections.Generic;
using System.Globalization;
using DumberCBRPatches.Configuration;
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
                Values[name] = new CharacterConfigEntries(name);
            }
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
        // TITS TYPE (SAFE CLAMP)
        // =========================================================
        public int TitsType
        {
            get
            {
                var val = ModSettingsDataRegister
                    .CharacterTitsDropdowns[CharacterName]
                    .Value;

                return val < 0 ? 0 : (val > 5 ? 5 : val);
            }
        }

        // =========================================================
        // RAW TRAITS STRING
        // =========================================================
        public string Traits =>
            ModSettings.GetString(BaseModName, $"{CharacterName}_Traits") ?? string.Empty;

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

                if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    continue;

                result[trait] = value;
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
    }
}