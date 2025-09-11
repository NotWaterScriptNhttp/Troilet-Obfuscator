using System;

namespace TroiletCore.Plugin
{
    public class PSettingText : IPluginSetting<string>
    {
        protected string _name = string.Empty;
        protected string _value = string.Empty;

        string IPluginSetting<string>.Name => throw new NotImplementedException();
        PluginSettingType IPluginSetting<string>.Type { get; set; } = PluginSettingType.Text;
        string IPluginSetting<string>.Value
        {
            get => _value;
            set => _value = value;
        }

        public PSettingText(string name, string value = "")
        {
            _name = name;
            _value = value;
        }
    }
}
