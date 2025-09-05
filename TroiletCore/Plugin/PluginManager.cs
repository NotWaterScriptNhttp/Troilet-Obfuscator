using System;
using System.IO;
using System.Runtime.Loader;

namespace TroiletCore.Plugin
{
    public static class PluginManager
    {
        private static readonly AssemblyLoadContext PCtx = new AssemblyLoadContext("Plugin context", true);

        public static readonly List<PluginBase> Plugins = new List<PluginBase>();

        private static PluginBase? LoadPlugin(string path)
        {
            return null;
        }

        public static void Init()
        {
            if (PCtx != null)
            {
                foreach (var p in Plugins)
                    p.OnUnload();

                Plugins.Clear();
                PCtx.Unload();
            }

            string plugins = Path.Combine(TroiletConfig.Instance.Root, "plugins");
            if (!Directory.Exists(plugins))
            {
                Directory.CreateDirectory(plugins);
                return;
            }


        }
    }
}
