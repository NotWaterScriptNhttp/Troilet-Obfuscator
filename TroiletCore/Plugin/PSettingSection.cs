using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class PSettingSection
    {
        private class SettingsConverter : JsonConverter<Dictionary<string, PluginSetting>>
        {
            public override Dictionary<string, PluginSetting>? ReadJson(JsonReader reader, Type objectType, Dictionary<string, PluginSetting>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return existingValue;
                if (existingValue == null)
                    return null;

                JObject o = (JObject)JToken.Load(reader);
                foreach (KeyValuePair<string, PluginSetting> kvp in existingValue)
                {
                    JToken? t = o.GetValue(kvp.Key);
                    if (t == null)
                        continue;

                    serializer.Populate(t.CreateReader(), kvp.Value);
                }

                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, Dictionary<string, PluginSetting>? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                JObject o = new JObject();
                foreach (KeyValuePair<string, PluginSetting> kvp in value)
                {
                    switch (kvp.Value.Type)
                    {
                        case PluginSettingType.Label:
                        case PluginSettingType.Button:
                            continue;
                    }

                    o.Add(kvp.Key, JToken.FromObject(kvp.Value, serializer));
                }

                o.WriteTo(writer);
            }
        }

        public string Name { get; private set; }
        [JsonProperty("Settings")]
        [JsonConverter(typeof(SettingsConverter))]
        public Dictionary<string, PluginSetting> Settings { get; private set; } = new Dictionary<string, PluginSetting>();

        private void AddSetting(PluginSetting setting) => Settings[setting.Name] = setting;

        public PSettingSection(string name)
        {
            Name = name;
        }

        public PSettingSection AddLabel(string name, string lbl, out PSettingLabel slbl)
        {
            AddSetting(slbl = new PSettingLabel(name, lbl));
            return this;
        }
        public PSettingSection AddLabel(string name, string lbl) => AddLabel(name, lbl, out _);

        public PSettingSection AddButton(string name, string lbl, Action onClick, out PSettingButton sbtn)
        {
            AddSetting(sbtn = new PSettingButton(name, lbl, onClick));
            return this;
        }
        public PSettingSection AddButton(string name, string lbl, Action onClick) => AddButton(name, lbl, onClick, out _);

        public PSettingSection AddToggle(string name, string lbl, bool val, out PSettingToggle stgl)
        {
            AddSetting(stgl = new PSettingToggle(name, lbl, val));
            return this;
        }
        public PSettingSection AddToggle(string name, string lbl, bool val) => AddToggle(name, lbl, val, out _);

        public PSettingSection AddTextInput(string name, string lbl, string val, out PSettingText stxt)
        {
            AddSetting(stxt = new PSettingText(name, lbl, val));
            return this;
        }
        public PSettingSection AddTextInput(string name, string lbl, string val) => AddTextInput(name, lbl, val, out _);

        public PSettingSection AddSlider(string name, string lbl, double min, double max, double val, double step, out PSettingRange srng)
        {
            AddSetting(srng = new PSettingRange(name, lbl, new SRange(min, max), val, step));
            return this;
        }
        public PSettingSection AddSlider(string name, string lbl, double min, double max, double val, double step = 1) => AddSlider(name, lbl, min, max, val, step, out _);

        public PSettingSection AddCombo(string name, string lbl, object[] options, object? val, out PSettingCombo scmb)
        {
            AddSetting(scmb = new PSettingCombo(name, lbl, options, val));
            return this;
        }
        public PSettingSection AddCombo(string name, string lbl, object[] options, object? val) => AddCombo(name, lbl, options, val, out _);
    }
}
