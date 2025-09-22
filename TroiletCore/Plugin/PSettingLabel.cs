using System;

namespace TroiletCore.Plugin
{
    public class PSettingLabel : PluginSetting, ISettingValue<string>
    {
        protected string _value = string.Empty;

        public event ISettingValue<string>.OnValueChange? OnChange;

        public override string Name { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Label;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public PSettingLabel(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
