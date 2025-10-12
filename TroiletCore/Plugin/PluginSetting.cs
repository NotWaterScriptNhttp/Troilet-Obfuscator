using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    internal class ValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(object);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else JToken.FromObject(value, serializer).WriteTo(writer);
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

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public interface ISettingValue<T>
    {
        public delegate void OnValueChange(T val);

        public event OnValueChange? OnChange;

        [JsonProperty]
        [JsonConverter(typeof(ValueConverter))]
        public T Value { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class PluginSetting
    {
        [JsonProperty]
        public abstract string Name { get; protected set; }
        public abstract string Label { get; protected set; }
        public abstract PluginSettingType Type { get; protected set; }
    }
}
