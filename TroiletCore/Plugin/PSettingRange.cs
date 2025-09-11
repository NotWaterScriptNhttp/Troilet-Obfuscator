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
    public class PSettingRange : IPluginSetting<int>
    {
        protected string _name = string.Empty;
        protected SRange _range = default;
        protected int _value = 0;

        string IPluginSetting<int>.Name => _name;
        PluginSettingType IPluginSetting<int>.Type { get; set; } = PluginSettingType.Range;
        int IPluginSetting<int>.Value
        {
            get => _value;
            set => _value = value;
        }

        public PSettingRange(string name, SRange range = default, int value = 0)
        {
            _name = name;
            _range = range;
            _value = value;
        }
    }
}
