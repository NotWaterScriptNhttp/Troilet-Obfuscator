using System;
using System.IO;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet
{
    [ProtectionLevel(ProtectionLevel.None, false)]
    public class RefResolver
    {
        private static Dictionary<string, List<TypeDef>> _namespaces = new();
        private static Dictionary<Type, TypeDef?> _cache = new();

        public static void LoadAssemblies(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            foreach (var lib in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var mdl = ModuleDefMD.Load(lib);
                    foreach (var t in mdl.Types)
                    {
                        if (t == mdl.GlobalType)
                            continue;

                        string ns = t.Namespace?.ToLower() ?? "";
                        if (!_namespaces.TryGetValue(ns, out var types))
                        {
                            types = new List<TypeDef>();
                            _namespaces[ns] = types;
                        }

                        types.Add(t);
                    }
                } catch(Exception _)
                {
                    Console.WriteLine("Failed to load assembly: {0}", Path.GetFileNameWithoutExtension(lib));
                }
            }
        }

        public static TypeDef? ResolveTypeNull(string ns, string name)
        {
            ns = ns.ToLower();
            name = name.ToLower();

            if (!_namespaces.TryGetValue(ns, out var types))
                return null;

            foreach (var t in types)
                if (t.Name.ToLower() == name)
                    return t;

            return null;
        }
        public static TypeDef? ResolveTypeNull<T>()
        {
            Type t = typeof(T);
            if (_cache.TryGetValue(t, out var type))
                return type;

            return _cache[t] = ResolveTypeNull(t.Namespace ?? "", t.Name);
        }

        public static void Clear() => _namespaces.Clear();
    }
}
