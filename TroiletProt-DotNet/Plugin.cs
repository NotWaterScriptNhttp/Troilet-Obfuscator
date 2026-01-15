using System;
using System.IO;

using dnlib.DotNet;

using TroiletCore;
using TroiletCore.Plugin;
using TroiletProt_DotNet.Controls;
using TroiletProt_DotNet.Protections;

namespace TroiletProt_DotNet
{
    public class Plugin : PluginBase, IObfuscatorPlugin
    {
        private AssemblyDef? LoadedFile = null;
        private Dictionary<int, List<ProtectionBase>> Protections;

        public override string Name => ".NET Obfuscator";
        public override string Description => "Troilet's .NET obfuscator";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);
        public override PluginConfigBase? Config { get; protected set; } = new PluginConfig();

        string IObfuscatorPlugin.Platform { get; set; } = ".NET";
        string[] IObfuscatorPlugin.PlatformExt { get; set; } = { "exe", "dll" };
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "dotnet", "dn" };

        public Stream? LoadFile(byte[] data)
        {            
            if (ExcludeWindow.Instance != null)
            {
                ExcludeWindow.Instance.Close();
                ExcludeWindow.Instance = null;
            }

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

            AssemblyDef asm = AssemblyDef.Load(file);
            foreach (ModuleDef mdl in asm.Modules)
            {
                foreach (KeyValuePair<int, List<ProtectionBase>> prots in Protections)
                {
                    Console.WriteLine("Running protection pass #{0}", prots.Key);

                    Dictionary<ProtectionBase, ProtectionSession> sessions = new();
                    foreach (ProtectionBase p in prots.Value)
                        sessions.Add(p, p.StartSession(mdl));

                    foreach (TypeDef t in mdl.Types)
                        foreach (KeyValuePair<ProtectionBase, ProtectionSession> kvp in sessions)
                            kvp.Key.OnType(kvp.Value, t);

                    foreach (KeyValuePair<ProtectionBase, ProtectionSession> kvp in sessions)
                        Console.WriteLine(kvp.Value.EndSession());
                }

                Console.WriteLine("Finished all passes!");
            }

            asm.Write(output);
            return true;
        }

        public override void OnLoad()
        {
            Protections = new()
            {
                { 0, new List<ProtectionBase>()
                    {
                        new Embedder(),
                        new ConstantProtection(),
                    } 
                },
                { 1, new List<ProtectionBase>()
                    {
                        new Renamer()
                    } 
                }
            };
        }
    }
}
