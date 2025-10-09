using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public PluginConfig()
        {
            AddLabel("label1", "Test Label");
            AddRange("Protection Level", 1, 0, 5);
            AddToggle("Enable Packing", true);
            AddTextInput("Encryption Key", "Test_key5583$$$");
        }
    }
}
