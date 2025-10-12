using System;

namespace TroiletCore.Plugin
{
    public class PSettingLabel : PluginSetting
    {
        public override string Name { get; protected set; } = string.Empty;
        public override string Label { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Label;

        public PSettingLabel(string name, string value)
        {
            Name = name;
            Label = value;
        }
    }
}
