using System;

using dnlib.DotNet;

using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class Plugin : PluginBase, IObfuscatorPlugin
    {
        public override string Name => ".NET Protector";
        public override string Description => "Allows troilet to obfuscate .NET assemblies";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);

        string IObfuscatorPlugin.Platform { get; set; } = ".NET";
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "dotnet", "dn" };

        public override void OnLoad()
        {
            Console.WriteLine(typeof(Plugin).Assembly.Location);
            AssemblyDef ad = AssemblyDef.Load(typeof(Plugin).Assembly.Location);
        }
    }
}
