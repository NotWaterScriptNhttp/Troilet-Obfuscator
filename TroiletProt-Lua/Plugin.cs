using System;
using System.IO;

using TroiletCore;
using TroiletCore.Plugin;

namespace TroiletProt_Lua
{
    public class Plugin : PluginBase, IObfuscatorPlugin
    {
        public override string Name => "Lua Obfuscator";
        public override string Description => "Troilet's Lua obfuscator";
        public override string Author => "Troilet Team";
        public override Version Version => new Version(1, 0);
        public override PluginConfigBase? Config { get; protected set; }

        string IObfuscatorPlugin.Platform { get; set; } = "Lua";
        string[] IObfuscatorPlugin.PlatformExt { get; set; } = { "lua" };
        string[]? IObfuscatorPlugin.ShortNames { get; set; } = { "lua" };

        public Stream? LoadFile(byte[] fileData)
        {
            using (BinaryReader br = new(new MemoryStream(fileData)))
                if (br.ReadUInt16() == 0x4D5A)
                    return null;

            return Utils.GetResourceStream("LuaIcon.png");
        }

        public bool Obfuscate(string file, string output, string[]? deps = null)
        {
            throw new NotImplementedException();
        }

        public override void OnLoad()
        {
            throw new NotImplementedException();
        }

        Stream? IObfuscatorPlugin.LoadFile(byte[] fileData)
        {
            throw new NotImplementedException();
        }

        bool IObfuscatorPlugin.Obfuscate(string file, string output, string[]? deps)
        {
            throw new NotImplementedException();
        }
    }
}
