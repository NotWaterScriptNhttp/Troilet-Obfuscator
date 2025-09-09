using System;

namespace TroiletCore.Plugin
{
    public interface IObfuscatorPlugin
    {
        public string Platform { get; protected set; }
        public string[]? ShortNames { get; protected set; }

        public bool Obfuscate(string file, string output, string[]? deps = null);
    }
}
