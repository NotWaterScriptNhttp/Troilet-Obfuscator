using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroiletCore.Plugin
{
    internal class SectionConverter : JsonConverter<PSettingSection>
    {
        public override PSettingSection? ReadJson(JsonReader reader, Type objectType, PSettingSection? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, PSettingSection? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                (new JObject()).WriteTo(writer);
                return;
            }

            JObject o = new JObject();
            foreach (KeyValuePair<string, PluginSetting> kvp in value.Settings)
            {
                // No need to save labels
                if (kvp.Value.Type == PluginSettingType.Label)
                    continue;

                o.Add(kvp.Key, JToken.FromObject(kvp.Value));
            }
            o.WriteTo(writer);
        }
    }

    [JsonConverter(typeof(SectionConverter))]
    public sealed class PSettingSection
    {
        public string Name { get; private set; }
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
