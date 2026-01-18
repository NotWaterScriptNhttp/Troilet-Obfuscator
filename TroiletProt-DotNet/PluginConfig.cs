using System;
using System.Windows;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using TroiletCore.Plugin;
using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Controls;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet
{
    [JsonObject(MemberSerialization.OptOut)]
    [ProtectionLevel(ProtectionLevel.Name)]
    public class Exclusion
    {
        public string Type = string.Empty;
        public bool ExcludeName = false;

        public string[]? Fields = null;
        public string[]? Properties = null;
        public string[]? Methods = null;
        public string[]? Events = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EmptyOrNull(Array? a) => a == null || a.Length == 0;

        [JsonIgnore]
        public bool IsFullExclusion => EmptyOrNull(Fields) && EmptyOrNull(Properties) && EmptyOrNull(Methods) && EmptyOrNull(Events);
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ProtectionLevel(ProtectionLevel.Name)]
    public class PluginConfig : PluginConfigBase
    {
        [ProtectionLevel(ProtectionLevel.Full, false)]
        public static PluginConfig? Instance { get; private set; } = null;

        [JsonProperty("Exclusions")]
        public List<Exclusion> Exclusions = new List<Exclusion>();

        public PluginConfig()
        {
            Instance = this;

            AddSection("Protections")
                .AddToggle("embed_prot", "Protect Embeds", true)
                .AddToggle("const_prot", "Protect Constants", true);

            AddSection("Misc")
                .AddButton("show_exclusions", "Open Exclusions", () =>
                {
                    if (ExcludeWindow.Instance == null)
                    {
                        MessageBox.Show("No .NET assembly loaded!", "Troilet", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ExcludeWindow.Instance.Show();
                });
        }
    }
}
