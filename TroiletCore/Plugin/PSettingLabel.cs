using System;

namespace TroiletCore.Plugin
{
    public class PSettingLabel : IPluginSetting<string>
    {
        protected string _name = string.Empty;
        protected string _value = string.Empty;

        string IPluginSetting<string>.Name => _name;
        PluginSettingType IPluginSetting<string>.Type { get; set; } = PluginSettingType.Label;
        string IPluginSetting<string>.Value
        {
            get => _value;
            set => _value = value;
        }

        public PSettingLabel(string name, string value)
        {
            _name = name;
            _value = value;
        }
    }
}
