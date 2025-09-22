using System;

namespace TroiletCore.Plugin
{
    public struct SRange
    {
        public int Start = 0;
        public int End = 0;

        public SRange(int s, int e)
        {
            Start = s; 
            End = e;
        }
    }
    public class PSettingRange : PluginSetting, ISettingValue<int>
    {
        protected SRange _range = default;
        protected int _value = 0;

        public event ISettingValue<int>.OnValueChange? OnChange;

        public override string Name { get; protected set; }
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Range;
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public PSettingRange(string name, SRange range = default, int value = 0)
        {
            Name = name;
            _range = range;
            Value = value;
        }
    }
}
