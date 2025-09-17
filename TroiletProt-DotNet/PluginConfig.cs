using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public PSettingLabel 
        public PSettingRange ProtectionLevel = new PSettingRange("Protection Level", new SRange(1, 5), 3);
    }
}
