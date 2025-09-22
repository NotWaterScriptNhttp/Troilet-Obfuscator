using System;

namespace TroiletCore.Plugin
{
    public class PSettingText : PluginSetting, ISettingValue<string>
    {
        protected string _value = string.Empty;

        public event ISettingValue<string>.OnValueChange? OnChange;

        public override string Name { get; protected set; }
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Text;
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

        public PSettingText(string name, string value = "")
        {
            Name = name;
            Value = value;
        }
    }
}
