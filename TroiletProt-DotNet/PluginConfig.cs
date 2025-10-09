using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public PluginConfig()
        {
            string[] test = new string[]
            {
                "Test",
                "test2",
                "lakomec5",
                "Skoc z okna"
            };

            AddLabel("label1", "Test Label");
            AddRange("Protection Level", 1, 0, 5);
            AddToggle("Enable Packing", true);
            AddTextInput("Encryption Key", "Test_key5583$$$");
            AddCombo("Dropdown1", test, test[1]);
            AddComboMulti("MultiDropdown1", test);
        }
    }
}
