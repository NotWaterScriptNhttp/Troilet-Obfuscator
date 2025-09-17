using System;
using System.Collections.Generic;

namespace TroiletCore.Plugin
{
    public abstract class PluginConfigBase
    {
        protected List<object> _Settings = new List<object>();
        public object[] Settings => _Settings.ToArray();

        protected void AddLabel(string name, string txt) => _Settings.Add(new PSettingLabel(name, txt));
        protected void AddToggle(string name, bool val = false) => _Settings.Add(new PSettingToggle(name, val));
        protected void AddTextInput(string name, string val = "") => _Settings.Add(new PSettingText(name, val));
        protected void AddRange(string name, int len, int val = 0, int start = 0) => _Settings.Add(new PSettingRange(name, new SRange(start, len), val));
        protected void AddCombo<T>(string name, T[] options, T? val) => _Settings.Add(new PSettingCombo<T>(name, options, val));
        protected void AddComboMulti<T>(string name, T[] options, T[]? vals) => _Settings.Add(new PSettingMultiCombo<T>(name, options, vals));
    }
}
