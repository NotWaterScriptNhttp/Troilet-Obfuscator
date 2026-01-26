using System;
using System.IO;

using dnlib.DotNet;

using TroiletCore.Plugin;
using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Controls;
using TroiletProt_DotNet.Attributes;
using TroiletProt_DotNet.Extensions;
using TroiletProt_DotNet.Protections;

namespace TroiletProt_DotNet
{
    [ProtectionLevel(ProtectionLevel.Name)]
    public class Plugin : PluginBase, IObfuscatorPlugin
    {
        [ProtectionLevel(ProtectionLevel.Full, false)]
        private AssemblyDef? LoadedFile = null;
        [ProtectionLevel(ProtectionLevel.Full, false)]
        private Dictionary<int, List<ProtectionBase>> Protections;

        // Default .NET versions (The most used ones)
        private const string DEFAULT_COREVER = "8.0";
        private const string DEFAULT_NETFRVER = "v4.7.2";
        private const string DEFAULT_NETSTANVER = "2.1";

        public override string Name => ".NET Obfuscator";
        public override string Description => "Troilet's .NET obfuscator";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);
        public override PluginConfigBase? Config { get; protected set; } = new PluginConfig();

        string IObfuscatorPlugin.Platform { get; set; } = ".NET";
        string[] IObfuscatorPlugin.PlatformExt { get; set; } = { "exe", "dll" };
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "dotnet", "dn" };

        [ProtectionLevel(ProtectionLevel.Name, false)]
        private static string GetAssembliesFolder(TargetPlatform plat, string? ver)
        {
            static string? CheckFolder(string bfldr, string rver, string def)
            {
                string s = Path.Combine(bfldr, rver);
                if (Directory.Exists(s))
                    return s;

                s = Path.Combine(bfldr, def);
                if (Directory.Exists(s))
                    return s;

                return null;
            }

            string? fldr = null;
            switch (plat)
            {
                case TargetPlatform.Core:
                    if (ver == null)
                        ver = DEFAULT_COREVER;

                    fldr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet/packs/Microsoft.NETCore.App.Ref");
                    if (!Directory.Exists(fldr))
                    {
                        fldr = null;
                        break;
                    }

                    string[] vers = Directory.GetDirectories(fldr, ver + ".*");
                    if (vers.Length <= 0)
                    {
                        fldr = null;
                        break;
                    }

                    fldr = CheckFolder(fldr, vers[0].Substring(vers[0].LastIndexOf('\\') + 1), DEFAULT_COREVER);
                    if (fldr == null)
                        break;

                    fldr = Path.Combine(fldr, $"ref/net{ver}");
                    break;

                case TargetPlatform.Framework:
                    if (ver == null)
                        ver = DEFAULT_NETFRVER;

                    fldr = CheckFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Reference Assemblies/Microsoft/Framework/.NETFramework"), ver, DEFAULT_NETFRVER);
                    break;

                case TargetPlatform.Standard:
                    if (ver == null)
                        ver = DEFAULT_NETSTANVER;

                    fldr = CheckFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet/packs/NETStandard.Library.Ref"), ver + ".0", DEFAULT_NETSTANVER + ".0");
                    if (fldr == null)
                        break;

                    fldr = Path.Combine(fldr, $"ref/netstandard{ver}");
                    break;

                default:
                    return "";
            }

            if (fldr == null)
                throw new ApplicationException("Failed to find folder with target assemblies!");

            return fldr;
        }

        Stream? IObfuscatorPlugin.LoadFile(byte[] data)
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

            byte[]? icon = Globals.ReadEmbed("Resources.ILIcon.png");
            return icon != null ? new MemoryStream(icon) : null;
        }
        bool IObfuscatorPlugin.Obfuscate(string file, string output, string[]? deps = null)
        {
            [ProtectionLevel(ProtectionLevel.Full, false)]
            static void CallOnType(Dictionary<ProtectionBase, ProtectionSession> sessions, TypeDef t, int depth = 0)
            {
                if (depth >= 10)
                {
                    Console.WriteLine("Maximum depth for types reached!");
                    return;
                }

                foreach (var nt in t.NestedTypes)
                    CallOnType(sessions, nt, depth + 1);

                foreach (KeyValuePair<ProtectionBase, ProtectionSession> kvp in sessions)
                    kvp.Key.OnType(kvp.Value, t);
            }

            if (LoadedFile == null)
                return false;

            Globals.Key = new byte[64];
            Globals.Salt = new byte[8];
            Globals.Rand.NextBytes(Globals.Key);
            Globals.Rand.NextBytes(Globals.Salt);
            Console.WriteLine("Key: {0}|{1}", Globals.Key.ToB64(), Globals.Salt.ToB64());

            ModuleDefMD mdl = ModuleDefMD.Load(file);

            // Find which version of .Net the module is using
            TargetPlatform platform = TargetPlatform.Unknown;
            string? netVersion = null;
            if (mdl.Assembly != null)
            {
                foreach (var ca in mdl.Assembly.CustomAttributes)
                    if (ca.TypeFullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                    {
                        string s = ca.ConstructorArguments[0].Value.ToString() ?? ",";
                        string[] p = s.Split(',');

                        string target = p[0].ToLower();
                        if (target == ".netcoreapp")
                            platform = TargetPlatform.Core;
                        else if (target == ".netframework")
                            platform = TargetPlatform.Framework;
                        else if (target == ".netstandard")
                            platform = TargetPlatform.Standard;
                        else throw new ApplicationException($"Unsupported target platform {target}");

                        string ver = p[1].Split('=')[1];
                        if (platform != TargetPlatform.Framework)
                            netVersion = ver.TrimStart('v');
                        else netVersion = ver;

                        break;
                    }
            }
            else
            {
                string lib = mdl.CorLibTypes.AssemblyRef.Name.ToLower();
                if (lib == "mscorlib")
                    platform = TargetPlatform.Framework;
                else if (lib == "system.private.corlib")
                    platform = TargetPlatform.Core;
            }

            RefResolver.LoadAssemblies(GetAssembliesFolder(platform, netVersion));

            // Add global cctor
            Globals.CCtor = new MethodBuilder(".cctor", mdl.CorLibTypes.Void, attrs: MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig);
            mdl.GlobalType.Methods.Add(Globals.CCtor.Get());

            foreach (KeyValuePair<int, List<ProtectionBase>> prots in Protections)
            {
                Console.WriteLine("Running protection pass #{0}", prots.Key);

                Dictionary<ProtectionBase, ProtectionSession> sessions = new();
                foreach (ProtectionBase p in prots.Value)
                {
                    if (!p.CanProtect()) // Check if we can use this protection (Check if its enabled)
                        continue;

                    sessions.Add(p, p.StartSession(mdl));
                }

                foreach (TypeDef t in mdl.Types)
                {
                    // Very likely most protections won't need to inspect global type
                    if (t == mdl.GlobalType)
                    {
                        foreach (KeyValuePair<ProtectionBase, ProtectionSession> kvp in sessions)
                            kvp.Key.OnGlobalType(kvp.Value, t);

                        continue;
                    }

                    CallOnType(sessions, t);
                }

                foreach (KeyValuePair<ProtectionBase, ProtectionSession> kvp in sessions)
                    Console.WriteLine("[{0}]: {1}", kvp.Key.Name, kvp.Value.EndSession());

                Console.WriteLine();
            }

            // End global cctor
            Globals.CCtor.AddInst(dnlib.DotNet.Emit.OpCodes.Ret);
            Globals.CCtor.Resolve();

            RefResolver.Clear();
            Globals.Clear();
            Console.WriteLine("Finished all passes for {0}", mdl.Name);

            mdl.Write(output);
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
                        new TypeSpooferProtection()
                    } 
                },
                { 2, new List<ProtectionBase>()
                    {
                        new Renamer()
                    } 
                }
            };
        }
    }
}
