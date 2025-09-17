using System;

namespace TroiletCore.Plugin
{
    public class PSettingLabel : PluginSetting, ISettingValue<string>
    {
        protected string _value = string.Empty;

        public override string Name { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public PSettingLabel(string name, string value)
        {
            Name = name;
            _value = value;
        }
    }
}
