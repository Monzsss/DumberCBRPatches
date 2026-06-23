using System;
using MBM.ModLoader.Settings;

namespace DumberCBRPatches.Configuration
{
    public class ProfessionConfigValues
    {
        private const string BaseModName = "ComplexBreedingRedux";

        public bool GetTeachable(string profName)
        {
            if (string.Equals(profName, "Maid", StringComparison.OrdinalIgnoreCase)) return IsEnabled("Maid_Teachable");
            if (string.Equals(profName, "Paladin", StringComparison.OrdinalIgnoreCase)) return IsEnabled("Paladin_Teachable");
            if (string.Equals(profName, "Warrior", StringComparison.OrdinalIgnoreCase)) return IsEnabled("Warrior_Teachable");
            if (string.Equals(profName, "Wizard", StringComparison.OrdinalIgnoreCase)) return IsEnabled("Wizard_Teachable");

            return false;
        }

        public int GetBaseChance(string profName)
        {
            if (string.Equals(profName, "Maid", StringComparison.OrdinalIgnoreCase)) return GetInt("Maid_BaseChance");
            if (string.Equals(profName, "Paladin", StringComparison.OrdinalIgnoreCase)) return GetInt("Paladin_BaseChance");
            if (string.Equals(profName, "Warrior", StringComparison.OrdinalIgnoreCase)) return GetInt("Warrior_BaseChance");
            if (string.Equals(profName, "Wizard", StringComparison.OrdinalIgnoreCase)) return GetInt("Wizard_BaseChance");

            return 0;
        }

        private static bool IsEnabled(string key)
        {
            return ModSettings.GetDropdown(BaseModName, key) == 1;
        }

        private static int GetInt(string key)
        {
            return ModSettings.GetInt(BaseModName, key);
        }
    }
}
