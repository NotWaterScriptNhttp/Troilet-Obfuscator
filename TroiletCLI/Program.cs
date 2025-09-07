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
            

            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
