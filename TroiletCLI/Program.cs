using System;

using TroiletCore;
using TroiletCore.Plugin;

namespace TroiletCLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new TroiletConfig();

            PluginManager.Init();

            PluginBase? p = PluginManager.GetObfuscator("dn");
            if (p == null)
            {
                Console.WriteLine("Failed to find dotnet protector!");
                return;
            }

            Directory.CreateDirectory("output");
            ((IObfuscatorPlugin)p).Obfuscate("plugins/TroiletProt-DotNet.dll", "output/Protected.dll");

            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
