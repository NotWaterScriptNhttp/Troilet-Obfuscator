using System;

namespace TroiletCore.Plugin
{
    public class PSettingToggle : PluginSetting, ISettingValue<bool>
    {
        protected bool _enabled = false;

        public event ISettingValue<bool>.OnValueChange? OnChange;

        public override string Name { get; protected set; } = string.Empty;
        public override string Label { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Toggle;
        public bool Value
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public PSettingToggle(string name, string lbl, bool defValue = false)
        {
            Name = name;
            Label = lbl;
            Value = defValue;
        }
    }
}
