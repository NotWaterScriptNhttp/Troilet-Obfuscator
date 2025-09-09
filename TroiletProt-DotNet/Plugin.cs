using System;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using TroiletCore.Plugin;

namespace TroiletProt_DotNet
{
    public class Plugin : PluginBase<PluginConfig>, IObfuscatorPlugin
    {
        public override string Name => ".NET Protector";
        public override string Description => "Allows troilet to obfuscate .NET assemblies";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);
        public override PluginConfig? Config { get; protected set; } = new PluginConfig();

        string IObfuscatorPlugin.Platform { get; set; } = ".NET";
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "dotnet", "dn" };

        public bool Obfuscate(string file, string output, string[]? deps = null)
        {
            AssemblyDef asm = AssemblyDef.Load(file);

            foreach (ModuleDef mdl in asm.Modules)
            {
                foreach (TypeDef t in mdl.Types)
                {
                    foreach (MethodDef m in t.Methods)
                    {
                        if (!m.HasBody || !m.Body.HasInstructions)
                            continue;

                        foreach (Instruction i in m.Body.Instructions)
                            if (i.OpCode.Code == Code.Ldstr)
                                i.Operand = "STRING!";
                    }
                }
            }

            asm.Write(output);
            return true;
        }

        public override void OnLoad()
        {
            Console.WriteLine(typeof(Plugin).Assembly.Location);
        }
    }
}
