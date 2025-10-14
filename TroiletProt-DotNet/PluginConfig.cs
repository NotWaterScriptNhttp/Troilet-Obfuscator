using System;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class PluginConfig : PluginConfigBase
    {
        public static PluginConfig? Instance { get; private set; } = null;

        public PluginConfig()
        {
            Instance = this;
        }
    }
}
