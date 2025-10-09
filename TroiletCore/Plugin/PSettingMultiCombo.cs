using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.Plugin
{
    public class PSettingMultiCombo : PluginSetting, ISettingValue<object[]?>
    {
        protected object[] _options;
        protected object[]? _value = null;

        public event ISettingValue<object[]?>.OnValueChange? OnChange;

        public override string Name { get; protected set; }
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Multicombo;
        public object[]? Value
        {
            get => _value;
            set
            {
                _value = value;
                if (OnChange != null)
                    OnChange.Invoke(value);
            }
        }

        public PSettingMultiCombo(string name, object[] options, object[]? values)
        {
            throw new NotImplementedException();

            Name = name;
            _options = options;
            Value = values;
        }
    }
}
