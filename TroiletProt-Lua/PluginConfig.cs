using System;
using Newtonsoft.Json;

using TroiletCore.Plugin;

namespace TroiletProt_Lua
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PluginConfig : PluginConfigBase
    {
        public static PluginConfig? Instance { get; private set; } = null;

        public PluginConfig()
        {
            Instance = this;


        }
    }
}
