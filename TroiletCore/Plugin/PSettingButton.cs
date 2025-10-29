using System;

namespace TroiletCore.Plugin
{
    public class PSettingButton : PluginSetting
    {
        private event Action OnClick;

        public override string Name { get; protected set; } = string.Empty;
        public override string Label { get; protected set; } = string.Empty;
        public override PluginSettingType Type { get; protected set; } = PluginSettingType.Button;


        public PSettingButton(string name, string value, Action onClick)
        {
            Name = name;
            Label = value;
            OnClick += onClick;
        }

        public void InvokeClick(object? s, EventArgs? ev) => OnClick?.Invoke();
    }
}
