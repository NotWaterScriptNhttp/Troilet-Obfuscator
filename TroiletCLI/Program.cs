using System;

using TroiletCore;
using TroiletCore.Plugin;

namespace TroiletCLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string[] args2 = new string[]
            {
                "uzenina3:",
                "path1:\"Mam rad peri.txt\"",
                "/xQc",
                "val:Cecek_Cecek1"
            };

            ArgParser parser = new ArgParser();
            parser.Parse(args2);

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
