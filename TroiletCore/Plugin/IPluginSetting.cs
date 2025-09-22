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

    public interface ISettingValue<T>
    {
        public delegate void OnValueChange(T val);

        public event OnValueChange? OnChange;
        public T Value { get; set; }
    }
    public abstract class PluginSetting
    {
        public abstract string Name { get; protected set; }
        public abstract PluginSettingType Type { get; protected set; }
    }
}
