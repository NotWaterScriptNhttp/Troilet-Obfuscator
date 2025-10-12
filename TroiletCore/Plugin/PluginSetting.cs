using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    internal class PluginSettingConverter : JsonConverter<PluginSetting>
    {
        public override PluginSetting? ReadJson(JsonReader reader, Type objectType, PluginSetting? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, PluginSetting? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            switch (value.Type)
            {
                case PluginSettingType.Toggle:
                    ISettingValue<bool> bv = (ISettingValue<bool>)value;
                    writer.WriteValue(bv.Value);
                    break;
                case PluginSettingType.Text:
                    ISettingValue<string> sv = (ISettingValue<string>)value;
                    writer.WriteValue(sv.Value);
                    break;
                case PluginSettingType.Range:
                    ISettingValue<double> nv = (ISettingValue<double>)value;
                    writer.WriteValue(nv.Value);
                    break;
                case PluginSettingType.Combo:
                    ISettingValue<object?> ov = (ISettingValue<object?>)value;
                    if (ov.Value == null)
                        writer.WriteNull();
                    else JToken.FromObject(ov.Value).WriteTo(writer);
                    break;
            }
        }
    }

    public enum PluginSettingType : byte
    {
        Label,
        Toggle,
        Text,
        Range,
        Combo
    }

    public interface ISettingValue<T>
    {
        public delegate void OnValueChange(T val);

        public event OnValueChange? OnChange;
        public T Value { get; set; }
    }

    [JsonConverter(typeof(PluginSettingConverter))]
    public abstract class PluginSetting
    {
        public abstract string Name { get; protected set; }
        public abstract string Label { get; protected set; }
        public abstract PluginSettingType Type { get; protected set; }
    }
}
