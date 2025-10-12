using System;

namespace TroiletCore.Plugin
{
    public struct SRange
    {
        public double Start = 0;
        public double End = 0;

        public SRange(double s, double e)
        {
            Start = s; 
            End = e;
        }
    }
    public class PSettingRange : PluginSetting, ISettingValue<double>
    {
        protected double _value = 0;

        public event ISettingValue<double>.OnValueChange? OnChange;

        public override string Name { get; protected set; } = string.Empty;
        public override string Label { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Range;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public SRange Range { get; private set; } = default;
        public double Step { get; private set; } = 1;

        public PSettingRange(string name, string lbl, SRange range = default, double value = 0, double step = 1)
        {
            Name = name;
            Label = lbl;
            Range = range;
            Value = value;
            Step = step;
        }
    }
}
