using System;

namespace TroiletCore.Plugin
{
    public enum PluginSettingType : byte
    {
        Label,
        Toggle,
        Text,
        Range,
        Combo,
        Multicombo
    }
    public interface IPluginSetting<T>
    {
        public string Name { get; }
        public PluginSettingType Type { get; protected set; }
        public T Value { get; set; }
    }
}
