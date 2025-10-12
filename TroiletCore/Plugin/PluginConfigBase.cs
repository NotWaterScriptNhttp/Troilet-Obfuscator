using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    internal class PluginConfigConverter : JsonConverter<PluginConfigBase>
    {
        public override PluginConfigBase? ReadJson(JsonReader reader, Type objectType, PluginConfigBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, PluginConfigBase? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            JObject sections = new JObject();
            foreach (KeyValuePair<string, PSettingSection> kvp in value._Sections)
                sections.Add(new JProperty(kvp.Key, JToken.FromObject(kvp.Value)));
            sections.WriteTo(writer);
        }
    }

    [JsonConverter(typeof(PluginConfigConverter))]
    public abstract class PluginConfigBase
    {
        internal Dictionary<string, PSettingSection> _Sections = new Dictionary<string, PSettingSection>();
        public PSettingSection[] Sections => _Sections.Values.ToArray();

        protected PSettingSection AddSection(string name)
        {
            if (_Sections.ContainsKey(name))
                throw new ApplicationException("This section already exists in the current config!");

            return _Sections[name] = new PSettingSection(name);
        }

        public string GetValue(string section, string setting, string defValue = "")
        {
            if (!_Sections.TryGetValue(section, out var sec))
                throw new ApplicationException($"Section '{section}' does not exist!");

            if (!sec.Settings.TryGetValue(setting, out var set))
                return defValue;

            switch (set.Type)
            {
                case PluginSettingType.Label:
                    return (set as ISettingValue<string>).Value;
                case PluginSettingType.Toggle:
                    return (set as ISettingValue<bool>).Value.ToString();
                case PluginSettingType.Text:
                    return (set as ISettingValue<string>).Value;
                case PluginSettingType.Range:
                    return (set as ISettingValue<double>).Value.ToString();
                case PluginSettingType.Combo:
                    return (set as ISettingValue<object?>).Value?.ToString();
            }

            return defValue;
        }
        public double GetValue(string section, string setting, double defValue = 0)
        {
            if (!_Sections.TryGetValue(section, out var sec))
                throw new ApplicationException($"Section '{section}' does not exist!");

            if (!sec.Settings.TryGetValue(setting, out var set))
                return defValue;

            switch (set.Type)
            {
                case PluginSettingType.Range:
                    return (set as ISettingValue<double>).Value;
            }

            return defValue;
        }
        public object? GetValue(string section, string setting, object? defValue = null)
        {
            if (!_Sections.TryGetValue(section, out var sec))
                throw new ApplicationException($"Section '{section}' does not exist!");

            if (!sec.Settings.TryGetValue(setting, out var set))
                return defValue;

            switch (set.Type)
            {
                case PluginSettingType.Label:
                    return (set as ISettingValue<string>).Value;
                case PluginSettingType.Toggle:
                    return (set as ISettingValue<bool>).Value;
                case PluginSettingType.Text:
                    return (set as ISettingValue<string>).Value;
                case PluginSettingType.Range:
                    return (set as ISettingValue<double>).Value;
                case PluginSettingType.Combo:
                    return (set as ISettingValue<object?>).Value;
            }

            return defValue;
        }
    }
}
