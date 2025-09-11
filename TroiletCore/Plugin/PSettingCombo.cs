using System;

namespace TroiletCore.Plugin
{
    public class PSettingCombo<T> : IPluginSetting<T?>
    {
        protected string _name = string.Empty;
        protected T[] _options;
        protected T? _value = default;

        string IPluginSetting<T?>.Name => _name;
        PluginSettingType IPluginSetting<T?>.Type { get; set; } = PluginSettingType.Combo;
        T? IPluginSetting<T?>.Value
        {
            get => _value; 
            set => _value = value;
        }

        public PSettingCombo(string name, T[] options, T? value = default)
        {
            _name = name;
            _options = options;
            _value = value;
        }
    }
}
