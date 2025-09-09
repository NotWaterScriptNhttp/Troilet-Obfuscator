using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace TroiletCore.Plugin
{
    public static class PluginManager
    {
        private class PluginLoadContext : AssemblyLoadContext
        {
            public PluginLoadContext() : base("Plugin context", true) {}

            protected override Assembly? Load(AssemblyName lan)
            {
                Assembly? asm = base.Load(lan);
                if (asm == null)
                {
                    string dep = Path.Combine(TroiletConfig.Instance.DependencyDir, $"{lan.Name}.dll");
                    if (File.Exists(dep))
                        asm = LoadFromAssemblyPath(dep);
                }

                return asm;
            }
            protected override nint LoadUnmanagedDll(string unmanagedDllName)
            {
                nint asm = base.LoadUnmanagedDll(unmanagedDllName);
                if (asm == nint.Zero)
                {
                    string dep = Path.Combine(TroiletConfig.Instance.DependencyDir, $"{unmanagedDllName}.dll");
                    if (File.Exists(dep))
                        asm = LoadUnmanagedDllFromPath(dep);
                }

                return asm;
            }
        }

        private static PluginLoadContext? PCtx = null;

        public static readonly List<PluginBase<IPluginConfig>> Plugins = new List<PluginBase<IPluginConfig>>();
        public static PluginBase<IPluginConfig>[] Obfuscators => Plugins.Where(x => x is IObfuscatorPlugin).ToArray();

        public static void Init()
        {
            if (PCtx != null)
            {
                foreach (var p in Plugins)
                    p.OnUnload();

                Plugins.Clear();
                PCtx.Unload();
            }

            PCtx = new PluginLoadContext();

            foreach (string f in Directory.GetFiles(TroiletConfig.Instance.PluginsDir))
            {
                Assembly pl = PCtx.LoadFromAssemblyPath(f);
                foreach (Type t in pl.ExportedTypes)
                {
                    if (t.BaseType != typeof(PluginBase<IPluginConfig>))
                        continue;

                    PluginBase<IPluginConfig>? pb = (PluginBase<IPluginConfig>?)Activator.CreateInstance(t);
                    if (pb == null)
                        continue;

                    Plugins.Add(pb);
                    break;
                }
            }

            // If a plugin requires another plugin
            foreach (var p in Plugins)
                p.OnLoad();
        }
        public static void Shutdown()
        {
            if (PCtx == null)
                return;

            foreach (var p in Plugins)
                p.OnUnload();

            Plugins.Clear();
            PCtx.Unload();
            PCtx = null;
        }

        public static PluginBase<IPluginConfig>? GetObfuscator(string platform)
        {
            platform = platform.ToLower();

            foreach (var p in Obfuscators)
            {
                IObfuscatorPlugin pl = p as IObfuscatorPlugin; // Cannot be null
                if (pl.Platform.ToLower() == platform)
                    return p;

                if (pl.ShortNames == null)
                    continue;

                foreach (var sn in pl.ShortNames)
                    if (sn.ToLower() == platform)
                        return p;
            }

            return null;
        }
        public static PluginBase<T>? GetObfuscator<T>(string platform) where T : IPluginConfig => GetObfuscator(platform) as PluginBase<T>;
    }
}
