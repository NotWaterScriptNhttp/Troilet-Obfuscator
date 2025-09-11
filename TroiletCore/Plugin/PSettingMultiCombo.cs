using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.Plugin
{
    public class PSettingMultiCombo<T> : IPluginSetting<T[]?>
    {
        protected string _name = string.Empty;
        protected T[] _options;
        protected T[]? _value = null;

        string IPluginSetting<T[]?>.Name => _name;
        PluginSettingType IPluginSetting<T[]?>.Type { get; set; } = PluginSettingType.Multicombo;
        T[]? IPluginSetting<T[]?>.Value
        {
            get => _value;
            set => _value = value;
        }

        public PSettingMultiCombo(string name, T[] options, T[]? values)
        {
            _name = name;
            _options = options;
            _value = values;
        }
    }
}
