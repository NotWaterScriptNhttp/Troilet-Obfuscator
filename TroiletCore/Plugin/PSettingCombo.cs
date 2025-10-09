using System;

namespace TroiletCore.Plugin
{
    public class PSettingCombo : PluginSetting, ISettingValue<object?>
    {
        protected object? _value = default;

        public event ISettingValue<object?>.OnValueChange? OnChange;

        public override string Name { get; protected set; }
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Combo;
        
        public object[] Options { get; private set; }
        public object? Value
        {
            get => _value;
            set
            {
                _value = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public PSettingCombo(string name, object[] options, object? value = default)
        {
            Name = name;
            Options = options;
            Value = value;
        }
    }
}
