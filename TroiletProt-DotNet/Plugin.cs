using System;
using System.IO;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TroiletCore;
using TroiletCore.Plugin;
using TroiletProt_DotNet.Controls;
using TroiletProt_DotNet.Protections;

namespace TroiletProt_DotNet
{
    public class Plugin : PluginBase, IObfuscatorPlugin
    {
        private AssemblyDef? LoadedFile = null;

        public override string Name => ".NET Obfuscator";
        public override string Description => "Troilet's .NET obfuscator";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);
        public override PluginConfigBase? Config { get; protected set; } = new PluginConfig();

        string IObfuscatorPlugin.Platform { get; set; } = ".NET";
        string[] IObfuscatorPlugin.PlatformExt { get; set; } = { "exe", "dll" };
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "dotnet", "dn" };

        private void CreateProtections(out List<ProtectionBase> prots)
        {

        }

        public Stream? LoadFile(byte[] data)
        {
            ExcludeWindow.Instance = null;

            try
            {     
                ExcludeWindow.Instance = new ExcludeWindow(LoadedFile = AssemblyDef.Load(data));
            } catch (BadImageFormatException _)
            {
                return null;
            }
            return Utils.GetResourceStream("ILIcon.png");
        }

        public bool Obfuscate(string file, string output, string[]? deps = null)
        {
            if (LoadedFile == null)
                return false;

            foreach (ModuleDef mdl in LoadedFile.Modules)
            {
                List<ProtectionBase> protections;
                CreateProtections(out protections);

                foreach (TypeDef t in mdl.Types)
                {
                    IHasCustomAttribute attrs = t;
                }
            }

            return true;
        }
    }
}
