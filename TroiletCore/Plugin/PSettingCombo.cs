using System;

namespace TroiletCore.Plugin
{
    public class PSettingCombo : PluginSetting, ISettingValue<object?>
    {
        protected object? _value = default;

        public event ISettingValue<object?>.OnValueChange? OnChange;

        public override string Name { get; protected set; } = string.Empty;
        public override string Label { get; protected set; } = string.Empty;
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

        public PSettingCombo(string name, string lbl, object[] options, object? value = default)
        {
            Name = name;
            Label = lbl;
            Options = options;
            Value = value;
        }
    }
}
