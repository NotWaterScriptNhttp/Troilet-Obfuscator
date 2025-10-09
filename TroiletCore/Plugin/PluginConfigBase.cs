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
        protected void AddRange(string name, double val, double min, double max) => _Settings.Add(new PSettingRange(name, new SRange(min, max), val));
        protected void AddCombo(string name, object[] options, object? val = null) => _Settings.Add(new PSettingCombo(name, options, val));
        protected void AddComboMulti(string name, object[] options, object[]? vals = null) => _Settings.Add(new PSettingMultiCombo(name, options, vals));
    }
}
