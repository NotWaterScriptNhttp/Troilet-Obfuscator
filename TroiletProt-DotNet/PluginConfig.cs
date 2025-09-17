using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public PluginConfig()
        {
            AddLabel("label1", "Test Label");
            AddRange("Protection Level", 5, 3, 1);
            AddToggle("Enable Packing", true);
            AddTextInput("Encryption Key", "Test_key5583$$$");
        }
    }
}
