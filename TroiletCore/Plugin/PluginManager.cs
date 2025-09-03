using System;
using System.IO;
using System.Runtime.Loader;

namespace TroiletCore.Plugin
{
    public static class PluginManager
    {
        public static AssemblyLoadContext PCtx { get; private set; } = new AssemblyLoadContext("Plugin context", true);

        private static PluginBase? LoadPlugin(string ppath)
        {
            return null;
        }

        public static void Init()
        {
            if (PCtx != null)
                PCtx.Unload();


        }
        public static PluginBase[]? InitPlugins()
        {
            return null;
        }
    }
}
