using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class PluginConfigBase
    {
        private class SectionsConverter : JsonConverter<Dictionary<string, PSettingSection>>
        {
            public override Dictionary<string, PSettingSection>? ReadJson(JsonReader reader, Type objectType, Dictionary<string, PSettingSection>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return existingValue;
                if (existingValue == null) 
                    return null;

                JObject o = (JObject)JToken.Load(reader);
                foreach (KeyValuePair<string, PSettingSection> kvp in existingValue)
                {
                    JToken? t = o.GetValue(kvp.Key);
                    if (t == null)
                        continue;

                    serializer.Populate(t.CreateReader(), kvp.Value);
                }

                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, Dictionary<string, PSettingSection>? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                JObject o = new JObject();
                foreach (KeyValuePair<string, PSettingSection> kvp in value)
                    o.Add(kvp.Key, JToken.FromObject(kvp.Value, serializer));

                o.WriteTo(writer);
            }
        }

        [JsonProperty("Sections")]
        [JsonConverter(typeof(SectionsConverter))]
        private Dictionary<string, PSettingSection> _Sections = new Dictionary<string, PSettingSection>();
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

        public string SaveAsJSON() => JsonConvert.SerializeObject(this);
        public void LoadAsJSON(string json)
        {
            JsonConvert.PopulateObject(json, this, new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Reuse
            });
        }
    }
}
