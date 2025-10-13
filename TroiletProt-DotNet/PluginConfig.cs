using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public static PluginConfig? Instance { get; private set; } = null;

        public PluginConfig()
        {
            string[] test = new string[]
            {
                "Test",
                "test2",
                "lakomec5",
                "Skoc z okna"
            };

            AddSection("General")
                .AddLabel("label1", "Test label")
                .AddSlider("protection_level", "Protection Level", 0, 5, 1)
                .AddToggle("enable_packing", "Enabled Packing", true)
                .AddTextInput("enc_key", "Encryption key", "KeyTestValue_$$$553==")
                .AddCombo("dropdown1", "Test dropdown", test, test[1]);

            Instance = this;
        }
    }
}
