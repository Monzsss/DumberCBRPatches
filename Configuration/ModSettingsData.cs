//ModSettingsData.cs

using MBM.ModLoader.Settings;

namespace DumberCBRPatches.Configuration
{
    public abstract class ModSettingsData
    {
        public string Name { get; }
        public string Description { get; }
        public string Placeholder { get; protected set; }

        protected ModSettingsData(string name, string description, string placeholder = "")
        {
            Name = name;
            Description = description;
            Placeholder = placeholder;
        }

        public abstract void Register(string group = "");
        public abstract void ForceReset();
    }

    public class ModSettingsDataDropdown : ModSettingsData
    {
        private readonly int _defaultIndex;
        public string[] Options { get; }

        public ModSettingsDataDropdown(string name, string[] options, int defaultIndex, string description, string placeholder = "")
            : base(name, description, placeholder)
        {
            Options = options;
            _defaultIndex = defaultIndex;
        }

        public int Value
        {
            get => ModSettings.GetDropdown(ModEntry.ModName, Name);
            set => ModSettings.Set(ModEntry.ModName, Name, value);
        }

        public override void Register(string group = "")
        {
            ModSettings.RegisterDropdown(ModEntry.ModName, Name, Options, _defaultIndex, Description, group);
        }

        public override void ForceReset() => Value = _defaultIndex;
    }

    public class ModSettingsDataInt : ModSettingsData
    {
        private readonly int _defaultValue;

        public ModSettingsDataInt(string name, int defaultValue, string description, string placeholder = "")
            : base(name, description, placeholder)
        {
            _defaultValue = defaultValue;
        }

        public int Value
        {
            get => ModSettings.GetInt(ModEntry.ModName, Name);
            set => ModSettings.Set(ModEntry.ModName, Name, value);
        }

        public override void Register(string group = "")
        {
            // Use placeholder if provided, otherwise use default
            string inputPlaceholder = !string.IsNullOrEmpty(Placeholder)
                ? Placeholder
                : "Enter an integer value.";

            ModSettings.RegisterInt(ModEntry.ModName, Name, _defaultValue, Description, inputPlaceholder, group);
        }

        public override void ForceReset() => Value = _defaultValue;
    }

    public class ModSettingsDataString : ModSettingsData
    {
        private readonly string _defaultValue;

        public ModSettingsDataString(string name, string defaultValue, string description, string placeholder = "")
            : base(name, description, placeholder)
        {
            _defaultValue = defaultValue;
        }

        public string Value
        {
            get => ModSettings.GetString(ModEntry.ModName, Name);
            set => ModSettings.Set(ModEntry.ModName, Name, value);
        }

        public override void Register(string group = "")
        {
            // Use placeholder if provided, otherwise use a generic one
            string inputPlaceholder = !string.IsNullOrEmpty(Placeholder)
                ? Placeholder
                : "Enter text here...";

            ModSettings.RegisterString(ModEntry.ModName, Name, _defaultValue, Description, inputPlaceholder, group);
        }

        public override void ForceReset() => Value = _defaultValue;
    }
}