using System;

namespace TroiletCore.Plugin
{
    public class PSettingToggle : IPluginSetting<bool>
    {
        protected string _name = string.Empty;
        protected bool _enabled = false;

        string IPluginSetting<bool>.Name => _name;
        PluginSettingType IPluginSetting<bool>.Type { get; set; } = PluginSettingType.Toggle;
        bool IPluginSetting<bool>.Value
        {
            get => _enabled; 
            set => _enabled = value;
        }

        public PSettingToggle(string name, bool defValue = false)
        {
            _name = name;
            _enabled = defValue;
        }
    }
}
