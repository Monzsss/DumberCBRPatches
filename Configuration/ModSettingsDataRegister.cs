using System;
using System.Collections.Generic;

namespace DumberCBRPatches.Configuration
{
    public static class ModSettingsDataRegister
    {
        private static readonly string[] TrueFalseLabels = { "False", "True" };
        private static readonly string[] ConceptionLabels = { "0%", "25%", "50%", "75%", "100%" };
        private static readonly string[] EssenceFilterLabels = { "Allow Negatives (Stock)", "Invert Negatives to Buffs" };

        public static readonly List<ModSettingsData> All = new();

        private static bool _initialized;
        private static bool _registered;

        public static readonly string[] TargetCharacters =
        {
            "Amilia2", "Anna", "Aure", "Barbara2", "Bella",
            "Claire", "Flora2", "Karen", "Lena2", "Nero",
            "Niel2", "Sena2", "Sylvia", "Vivi"
        };

        public static readonly string[] CustomTitsSizes =
        {
            "Flat", "Small", "Average", "Big", "Huge", "Gigantic"
        };

        public static ModSettingsDataDropdown EssenceFilterModeData { get; private set; } = null!;

        public static ModSettingsDataDropdown ConceptionRateData { get; private set; } = null!;
        public static ModSettingsDataInt SexTimeData { get; private set; } = null!;

        public static ModSettingsDataString PlayerTraitsData { get; private set; } = null!;

        public static ModSettingsDataDropdown MaidTeachableData { get; private set; } = null!;
        public static ModSettingsDataInt MaidChanceData { get; private set; } = null!;

        public static ModSettingsDataDropdown PaladinTeachableData { get; private set; } = null!;
        public static ModSettingsDataInt PaladinChanceData { get; private set; } = null!;

        public static ModSettingsDataDropdown WarriorTeachableData { get; private set; } = null!;
        public static ModSettingsDataInt WarriorChanceData { get; private set; } = null!;

        public static ModSettingsDataDropdown WizardTeachableData { get; private set; } = null!;
        public static ModSettingsDataInt WizardChanceData { get; private set; } = null!;

        public static Dictionary<string, ModSettingsDataString> CharacterTraits { get; }
            = new(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<string, ModSettingsDataDropdown> CharacterTitsDropdowns { get; }
            = new(StringComparer.OrdinalIgnoreCase);

        // =========================
        // MAIN ENTRY POINT
        // =========================
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            BuildSettings();
            RegisterAll();
        }

        // =========================
        // BUILD ALL SETTINGS
        // =========================
        private static void BuildSettings()
        {
            EssenceFilterModeData =
                new ModSettingsDataDropdown("Essence FilterMode", EssenceFilterLabels, 1, "Negative Effects Management", "Choose how to treat negative trait debuffs");

            PlayerTraitsData =
                new ModSettingsDataString("Player Traits", "Demonic=10,Sacred=10,Elemental=10,Eternal=10,Magical=10,Feral=10", "Player Traits");

            ConceptionRateData =
                new ModSettingsDataDropdown("Player ConceptionRate", ConceptionLabels, 3, "Player Conception Rate");

            SexTimeData =
                new ModSettingsDataInt("Player SexTime", 15, "Player Sex Time");

            MaidTeachableData =
                new ModSettingsDataDropdown("Maid Teachable", TrueFalseLabels, 1, "Maid Profession");

            MaidChanceData =
                new ModSettingsDataInt("Maid BaseChance", 40, "Maid Teach Chance");

            PaladinTeachableData =
                new ModSettingsDataDropdown("Paladin Teachable", TrueFalseLabels, 1, "Paladin Profession");

            PaladinChanceData =
                new ModSettingsDataInt("Paladin BaseChance", 5, "Paladin Teach Chance");

            WarriorTeachableData =
                new ModSettingsDataDropdown("Warrior Teachable", TrueFalseLabels, 0, "Warrior Profession");

            WarriorChanceData =
                new ModSettingsDataInt("Warrior BaseChance", 0, "Warrior Teach Chance");

            WizardTeachableData =
                new ModSettingsDataDropdown("Wizard Teachable", TrueFalseLabels, 0, "Wizard Profession");

            WizardChanceData =
                new ModSettingsDataInt("Wizard BaseChance", 0, "Wizard Teach Chance");

            AddCore();

            BuildCharacterSettings();
        }

        // =========================
        // CORE SETTINGS
        // =========================
        private static void AddCore()
        {
            All.Add(EssenceFilterModeData);
            
            All.Add(PlayerTraitsData);
            All.Add(ConceptionRateData);
            All.Add(SexTimeData);

            All.Add(MaidTeachableData);
            All.Add(MaidChanceData);

            All.Add(PaladinTeachableData);
            All.Add(PaladinChanceData);

            All.Add(WarriorTeachableData);
            All.Add(WarriorChanceData);

            All.Add(WizardTeachableData);
            All.Add(WizardChanceData);
        }

        // =========================
        // CHARACTER SETTINGS
        // =========================
        private static void BuildCharacterSettings()
        {
            // Define unique default sizes for each target character
            var defaultTitsSizes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Amilia2", "Gigantic" },
                { "Anna", "Huge" },
                { "Aure", "Huge" },
                { "Barbara2", "Huge" },
                { "Bella", "Big" },
                { "Claire", "Flat" },
                { "Flora2", "Big" },
                { "Karen", "Big" },
                { "Lena2", "Big" },
                { "Nero", "Huge" },
                { "Niel2", "Flat" },
                { "Sena2", "Big" },
                { "Sylvia", "Huge" },
                { "Vivi", "Small" }
            };

            // Shared default essence configuration string
            string defaultTraitsString = "Demonic=10,Sacred=10,Elemental=10,Eternal=10,Magical=10,Feral=10";

            foreach (var name in TargetCharacters)
            {
                // Find the index of the string layout, fallback to 2 (Average) if not found
                int defaultIndex = 2;
                if (defaultTitsSizes.TryGetValue(name, out string sizeName))
                {
                    int foundIndex = Array.IndexOf(CustomTitsSizes, sizeName);
                    if (foundIndex != -1)
                    {
                        defaultIndex = foundIndex;
                    }
                }

                var tits = new ModSettingsDataDropdown(
                    $"{name}_TitsType",
                    CustomTitsSizes,
                    defaultIndex,
                    $"{name} Breast Size"
                );

                var traits = new ModSettingsDataString(
                    $"{name}_Traits",
                    defaultTraitsString,
                    $"{name} Essence Traits"
                );


                CharacterTitsDropdowns[name] = tits;
                CharacterTraits[name] = traits;

                All.Add(tits);
                All.Add(traits);
            }
        }

        // =========================
        // REGISTER ALL SETTINGS
        // =========================
        public static void RegisterAll()
        {
            if (_registered)
                return;

            _registered = true;

            foreach (var setting in All)
            {
                setting.Register();
            }
        }

        // =========================
        // RESET
        // =========================
        public static void ExecuteGlobalReset()
        {
            foreach (var d in All)
            {
                d.ForceReset();
            }
        }

        // =========================
        // GETTERS
        // =========================
        public static ModSettingsDataDropdown? GetCharacterTits(string name)
        {
            return CharacterTitsDropdowns.TryGetValue(name, out var data) ? data : null;
        }

        public static ModSettingsDataString? GetCharacterTraits(string name)
        {
            return CharacterTraits.TryGetValue(name, out var data) ? data : null;
        }
    }
}
