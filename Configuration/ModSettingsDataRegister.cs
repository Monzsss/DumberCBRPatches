//ModSettingsDataRegister.cs

namespace DumberCBRPatches.Configuration
{
    public static class ModSettingsDataRegister
    {
        private static readonly string[] TrueFalseLabels = { "False", "True" };
        private static readonly string[] ConceptionLabels = { "0%", "25%", "50%", "75%", "100%" };
        private static readonly string[] EssenceFilterLabels = { "Allow Negatives", "Remove Negative Attributes" };

        public static readonly List<ModSettingsData> All = [];
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

        private static readonly Dictionary<string, string> CleanNameCache = new(StringComparer.OrdinalIgnoreCase);

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

        public static Dictionary<string, ModSettingsDataString> CharacterTraits { get; } = new(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, ModSettingsDataDropdown> CharacterTitsDropdowns { get; } = new(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            PrecomputeCleanNames();
            BuildSettings();
        }

        private static void PrecomputeCleanNames()
        {
            foreach (var name in TargetCharacters)
            {
                CleanNameCache[name] = ProcessCleanName(name);
            }
        }

        public static string ToDisplayNameHelper(string id)
        {
            if (CleanNameCache.TryGetValue(id, out string cachedName)) return cachedName;
            return ProcessCleanName(id);
        }

        private static string ProcessCleanName(string id)
        {
            for (int i = id.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(id[i])) return id.Substring(0, i + 1);
            }
            return id;
        }

        private static void BuildSettings()
        {
            EssenceFilterModeData = new ModSettingsDataDropdown("Essence Attributtes Mode", EssenceFilterLabels, 1, "Configure essence attributtes");
            PlayerTraitsData = new ModSettingsDataString("Player Essence", "Demonic=10,Sacred=10,Elemental=10,Eternal=10,Magical=10,Feral=10", "Configure player essence attributtes");
            ConceptionRateData = new ModSettingsDataDropdown("Player Conception Rate", ConceptionLabels, 3, "Configure player conception rates");
            SexTimeData = new ModSettingsDataInt("Player Sex Duration", 15, "Configure sex scene duration");

            MaidTeachableData = new ModSettingsDataDropdown("Maid Teachable", TrueFalseLabels, 1, "Maid Profession option");
            MaidChanceData = new ModSettingsDataInt("Maid Base Chance", 40, "Base teach rates percentage roll");
            PaladinTeachableData = new ModSettingsDataDropdown("Paladin Teachable", TrueFalseLabels, 1, "Paladin Profession option");
            PaladinChanceData = new ModSettingsDataInt("Paladin Base Chance", 5, "Base teach rates percentage roll");
            WarriorTeachableData = new ModSettingsDataDropdown("Warrior Teachable", TrueFalseLabels, 0, "Warrior Profession option");
            WarriorChanceData = new ModSettingsDataInt("Warrior Base Chance", 0, "Base teach rates percentage roll");
            WizardTeachableData = new ModSettingsDataDropdown("Wizard Teachable", TrueFalseLabels, 0, "Wizard Profession option");
            WizardChanceData = new ModSettingsDataInt("Wizard Base Chance", 0, "Base teach rates percentage roll");

            AddCore();
            BuildCharacterSettings();
            RegisterAll();
        }

        private static void AddCore()
        {
            EssenceFilterModeData.Register("Global Tweaks");
            PlayerTraitsData.Register("Player Config");
            ConceptionRateData.Register("Player Config");
            SexTimeData.Register("Player Config");

            All.Add(EssenceFilterModeData); All.Add(PlayerTraitsData); All.Add(ConceptionRateData); All.Add(SexTimeData);

            All.Add(MaidTeachableData); All.Add(MaidChanceData);
            All.Add(PaladinTeachableData); All.Add(PaladinChanceData);
            All.Add(WarriorTeachableData); All.Add(WarriorChanceData);
            All.Add(WizardTeachableData); All.Add(WizardChanceData);
        }

        private static void BuildCharacterSettings()
        {
            var defaultTitsSizes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Amilia2", "Gigantic" }, { "Anna", "Huge" }, { "Aure", "Huge" }, { "Barbara2", "Huge" }, { "Bella", "Big" },
                { "Claire", "Flat" }, { "Flora2", "Big" }, { "Karen", "Big" }, { "Lena2", "Big" }, { "Nero", "Huge" },
                { "Niel2", "Flat" }, { "Sena2", "Big" }, { "Sylvia", "Huge" }, { "Vivi", "Small" }
            };

            string defaultTraitsString = "Demonic=10,Sacred=10,Elemental=10,Eternal=10,Magical=10,Feral=10";

            foreach (var name in TargetCharacters)
            {
                string displayName = ToDisplayNameHelper(name);
                int defaultIndex = 2;

                if (defaultTitsSizes.TryGetValue(name, out string? sizeName))
                {
                    int foundIndex = Array.IndexOf(CustomTitsSizes, sizeName);
                    if (foundIndex != -1) defaultIndex = foundIndex;
                }

                var tits = new ModSettingsDataDropdown($"{displayName} Breast Size", CustomTitsSizes, defaultIndex, $"Configure Breast size for {displayName}");
                var traits = new ModSettingsDataString($"{displayName} Essence", defaultTraitsString, $"Configure essence attribute for {displayName}");

                CharacterTitsDropdowns[name] = tits;
                CharacterTraits[name] = traits;
                CharacterTitsDropdowns[displayName] = tits;
                CharacterTraits[displayName] = traits;

                All.Add(tits);
                All.Add(traits);
            }
        }

        public static void RegisterAll()
        {
            if (_registered) return;
            _registered = true;

            foreach (var item in All)
            {
                if (item.Name.Contains("Teachable") || item.Name.Contains("Chance"))
                {
                    item.Register("Professions Config");
                }
                else if (item.Name.Contains("Breast Size") || item.Name.Contains("Essence"))
                {
                    item.Register("Characters Modification");
                }
            }
        }

        public static void ExecuteGlobalReset()
        {
            foreach (var item in All)
            {
                item.ForceReset();
            }
        }
    }
}
