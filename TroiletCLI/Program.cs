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

            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
